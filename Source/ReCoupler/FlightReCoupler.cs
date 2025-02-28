﻿/*
	This file is part of ReCoupler /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2017-2022 Booots <David Boots>

	ReCoupler is double licensed, as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	ReCoupler /L Unleashed is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with ReCoupler /L Unleashed. If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with ReCoupler /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ObservableCollection;

namespace ReCoupler
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FlightReCoupler : MonoBehaviour
    {
        public static FlightReCoupler Instance;

        public Dictionary<Vessel, ObservableCollection<FlightJointTracker>> trackedJoints = new Dictionary<Vessel, ObservableCollection<FlightJointTracker>>();
        public List<FlightJointTracker> AllJoints
        {
            get
            {
                if (_dictChanged)
                {
                    _allJoints.Clear();
                    foreach (IList<FlightJointTracker> joints in trackedJoints.Values)
                    {
                        _allJoints.AddRange(joints);
                    }
                    _dictChanged = false;
                }
                return _allJoints;
            }
        }
        private List<FlightJointTracker> _allJoints = new List<FlightJointTracker>();
        private bool _dictChanged = false;
        private Dictionary<int, Coroutine> delayedUpdates = new Dictionary<int, Coroutine>();
        
        public void Awake()
        {
            if (Instance)
                Destroy(Instance);
            Instance = this;
            
            GameEvents.onVesselGoOffRails.Add(OnVesselGoOffRails);
            GameEvents.onVesselCreate.Add(OnVesselCreate);
            GameEvents.onVesselPartCountChanged.Add(OnVesselPartCountChanged);
            GameEvents.onJointBreak.Add(OnJointBreak);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            GameEvents.onPartDie.Add(OnPartDie);

        }

        public void OnDestroy()
        {
            GameEvents.onVesselGoOffRails.Remove(OnVesselGoOffRails);
            GameEvents.onVesselCreate.Remove(OnVesselCreate);
            GameEvents.onVesselPartCountChanged.Remove(OnVesselPartCountChanged);
            GameEvents.onJointBreak.Remove(OnJointBreak);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onPartDie.Remove(OnPartDie);
        }

        private void OnVesselDestroy(Vessel vessel)
        {
            if (!trackedJoints.ContainsKey(vessel))
                return;
            ClearJoints(vessel);    // Also removes it from the dictionary.
        }

        private void OnPartDie(Part part)
        {
            List<FlightJointTracker> jointsWithPart = AllJoints.FindAll((FlightJointTracker jt) => jt.parts.Contains(part));
            for (int i = jointsWithPart.Count - 1; i >= 0; i--)
            {
                jointsWithPart[i].Destroy();
                if (trackedJoints.ContainsKey(part.vessel))
                    trackedJoints[part.vessel].Remove(jointsWithPart[i]);
                else
                    Log.error("The destroyed part's vessel was not in the dictionary.");
            }
        }

        private void OnVesselCreate(Vessel vessel)
        {
            if (!vessel.loaded || vessel.packed)
                return;
            if (!delayedUpdates.ContainsKey(Time.frameCount))
                delayedUpdates.Add(Time.frameCount, StartCoroutine(DelayedUpdate(Time.frameCount)));
        }

        private void OnVesselPartCountChanged(Vessel vessel)
        {
            if (!vessel.loaded || vessel.packed)
                return;
            UpdateVessel(vessel);
            if (!delayedUpdates.ContainsKey(Time.frameCount))
                delayedUpdates.Add(Time.frameCount, StartCoroutine(DelayedUpdate(Time.frameCount)));
        }

        IEnumerator DelayedUpdate(int time)
        {
            yield return new WaitForFixedUpdate();
            Log.dbg("Running DelayedUpdate: {0}", Planetarium.GetUniversalTime());
            List<Vessel> vessels = trackedJoints.Keys.ToList();
            for (int i = vessels.Count - 1; i >= 0; i--)
            {
                UpdateVessel(vessels[i]);
                bool combinedAny = false;
                FlightJointTracker jt;
                for (int j = trackedJoints[vessels[i]].Count - 1; j >= 0; j--)
                {
                    jt = trackedJoints[vessels[i]][j];

                    if (jt.parts[0] == null || jt.parts[1] == null)
                    {
                        Log.dbg("Removing a null joint.");
                        jt.Destroy();
                        trackedJoints[vessels[i]].Remove(jt);
                        continue;
                    }
                    if (!jt.LinkCreated)
                    {
                        Log.dbg("A joint must have broken.");
                        jt.Destroy();
                        trackedJoints[vessels[i]].Remove(jt);
                        continue;
                    }
                    else if (jt.IsTrackingDockingPorts)
                    {
                        ReCouplerUtils.hasDockingPort(jt.nodes[0], out ModuleDockingNode fromNode);
                        ReCouplerUtils.hasDockingPort(jt.nodes[1], out ModuleDockingNode toNode);
                        /*if (dockingNode.state != dockingNode.st_docked_dockee.name &&
                            dockingNode.state != dockingNode.st_docked_docker.name &&
                            dockingNode.state != dockingNode.st_docker_sameVessel.name &&
                            dockingNode.state != dockingNode.st_preattached.name)*/
                        if ((fromNode != null && fromNode.otherNode != toNode) && (toNode != null && toNode.otherNode != fromNode))
                        {
                            Log.dbg("A joint must have undocked.");
                            trackedJoints[vessels[i]].Remove(jt);
                            continue;
                        }
                    }

                    if (jt.parts[0].vessel != jt.parts[1].vessel)
                    {
                        Log.dbg("Vessel tree split, but should be re-combined.");
                        Log.dbg("{0} / {1}", jt.parts[0].name, jt.parts[1].name);
                        Log.dbg("{0} / {1}", jt.parts[0].vessel, jt.parts[1].vessel);
                        // The vessel split at the official tree, but isn't split at this joint.

                        if (jt.Couple(ownerVessel: vessels[i]))
                        {
                            Log.dbg("Removing joint since it is now a real, coupled joint.");
                            Log.dbg("Parent0: {0} ;  Parent1: {1}", jt.parts[0].parent.name, jt.parts[1].parent.name);
                            trackedJoints[vessels[i]].Remove(jt);
                        }
                        else
                            Log.error("Could not couple parts!");

                        combinedAny = true;
                        continue;
                    }
                    else if (jt.parts[0].vessel != vessels[i] && jt.parts[1].vessel != vessels[i])
                    {
                        Log.dbg("Removing joint between {0} and {1} because they are no longer part of this vessel.", jt.parts[0].name, jt.parts[1].name);
                        if (jt.parts[0].vessel == jt.parts[1].vessel)
                        {
                            Log.dbg("Adding them to vessel {0} instead.", jt.parts[0].vessel.vesselName);
                            if (!trackedJoints.ContainsKey(jt.parts[0].vessel))
                                AddNewVesselToDict(jt.parts[0].vessel);
                            trackedJoints[jt.parts[0].vessel].Add(jt);
                            jt.CombineCrossfeedSets();
                        }
                        else
                            jt.Destroy();
                        trackedJoints[vessels[i]].Remove(jt);
                        continue;
                    }
                }

                if (combinedAny)
                {
                    for (int j = trackedJoints[vessels[i]].Count - 1; j >= 0; j--)
                    {
                        trackedJoints[vessels[i]][j].CombineCrossfeedSets();
                    }
                }
            }
            CheckActiveVessels();
            delayedUpdates.Remove(time);
        }

        private void OnVesselGoOffRails(Vessel vessel) { Log.dbg("onVesselGoOffRails: {0}", vessel.vesselName); CheckActiveVessels(); }

        private void OnJointBreak(EventReport data)
        {
            Log.dbg("onJointBreak: {0} on {1}", data.origin.name, data.origin.vessel.vesselName);
            Part brokenPart = data.origin;
            if (!trackedJoints.ContainsKey(brokenPart.vessel))
                return;
            IList<FlightJointTracker> joints = trackedJoints[brokenPart.vessel];
            for (int i = joints.Count - 1; i >= 0; i--)
            {
                if (joints[i].parts.Contains(brokenPart))
                {
                    if (!joints[i].LinkCreated)
                    {
                        joints[i].Destroy();
                        trackedJoints[brokenPart.vessel].Remove(joints[i]);
                    }
                    else
                    {
                        this.StartCoroutine(DelayedBreak(joints[i], brokenPart.vessel));
                    }
                }
            }
        }

        IEnumerator DelayedBreak(FlightJointTracker joint, Vessel owner)
        {
            yield return new WaitForFixedUpdate();
            Log.dbg("Running DelayedBreak on {0}", owner.vesselName);
            if (!joint.LinkCreated)
            {
                joint.Destroy();
                trackedJoints[owner].Remove(joint);
            }
        }

        public void UpdateVessel(Vessel vessel)
        {
            if (!vessel.loaded || vessel.packed)
                return;
            if (!trackedJoints.ContainsKey(vessel))
                AddNewVesselToDict(vessel);
            IList<FlightJointTracker> joints = trackedJoints[vessel];

            for (int j = joints.Count - 1; j >= 0; j--)
            {
                if (joints[j].Decouplers.Any(d => d.isDecoupled))
                {
                    Log.dbg("Decoupler {0} decoupled. Removing from joints list.", joints[j].Decouplers.First(d => d.isDecoupled).part.name);
                    joints[j].Destroy();
                    this.StartCoroutine(DelayedDecouplerCheck(joints[j], vessel));
                    joints.RemoveAt(j);
                    continue;
                }
            }
        }

        IEnumerator DelayedDecouplerCheck(FlightJointTracker joint, Vessel owner)
        {
            FixedJoint[] tempJoints = new FixedJoint[2];
            if (joint.ParentStillValid(0))
            {
                tempJoints[0] = joint.parts[0].gameObject.AddComponent<FixedJoint>();
                tempJoints[0].connectedBody = joint.parents[0].part.Rigidbody;
            }
            if (joint.ParentStillValid(1))
            {
                tempJoints[1] = joint.parts[1].gameObject.AddComponent<FixedJoint>();
                tempJoints[1].connectedBody = joint.parents[1].part.Rigidbody;
            }
            yield return new WaitForFixedUpdate();
            if (tempJoints[0] != null)
                DestroyImmediate(tempJoints[0]);
            if (tempJoints[1] != null)
                DestroyImmediate(tempJoints[1]);
            Log.dbg("Running DelayedDecouplerCheck on {0}", owner.vesselName);
            joint.PostDecouplerCheck();
        }

        public void CheckActiveVessels()
        {
            List<Vessel> activeVessels = FlightGlobals.VesselsLoaded;
            List<Vessel> trackedVessels = trackedJoints.Keys.ToList();
            for (int i = trackedVessels.Count - 1; i >= 0; i--)
            {
                try
                {
                    for (int j = trackedJoints[trackedVessels[i]].Count - 1; j >= 0; j--)
                    {
                        if (trackedJoints[trackedVessels[i]][j].parts[0].vessel != trackedVessels[i])
                        {
                            if (trackedJoints.ContainsKey(trackedJoints[trackedVessels[i]][j].parts[0].vessel))
                            {
                                trackedJoints[trackedJoints[trackedVessels[i]][j].parts[0].vessel].Add(trackedJoints[trackedVessels[i]][j]);
                                trackedJoints[trackedVessels[i]].RemoveAt(j);
                            }
                            else
                                Log.warn("The Vessel a JointTracker should belong to has not yet been added to the dictionary.");
                        }
                    }
                    if (!activeVessels.Contains(trackedVessels[i]))
                    {
                        Log.dbg("Vessel {0} is no longer loaded. Removing it from the dictionary.", trackedVessels[i].name);
                        OnVesselDestroy(trackedVessels[i]);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Log.error(ex, "while checking loaded vessels.");
                }
            }
            for (int i = activeVessels.Count - 1; i >= 0; i--)
            {
                if (trackedJoints.ContainsKey(activeVessels[i]))
                    continue;
                GenerateJoints(activeVessels[i]);
            }
        }

        public void GenerateJoints(Vessel vessel)
        {
            Log.dbg("generateJoints: {0}", vessel.vesselName);
            if (!trackedJoints.ContainsKey(vessel))
                AddNewVesselToDict(vessel);
            List<Part> vesselParts = vessel.parts;

            List<Part> childen;
            Part activePart;
            for (int p = 0; p < vesselParts.Count; p++)
            {
                activePart = vesselParts[p];
                childen = activePart.FindChildParts<Part>(false).ToList();
                if (activePart.parent != null)
                    childen.Add(activePart.parent);
                for (int n = 0; n < activePart.attachNodes.Count; n++)
                {
                    if(activePart.attachNodes[n].attachedPart!=null && !childen.Contains(activePart.attachNodes[n].attachedPart))
                    {
                        ParseAttachNodes(activePart.attachNodes[n], activePart.attachNodes[n].attachedPart.attachNodes.FindAll(AN => AN.attachedPart == activePart), vessel);
                    }
                }
            }
        }

        private void ParseAttachNodes(AttachNode parentNode, List<AttachNode> childNodes, Vessel vessel = null)
        {
            if (vessel == null)
                vessel = parentNode.owner.vessel;
            for (int i = 0; i < childNodes.Count; i++)
            {
                FlightJointTracker existingTracker = AllJoints.FirstOrDefault((FlightJointTracker jt) => jt.nodes.Contains(parentNode) && jt.nodes.Contains(childNodes[i]));
                if (existingTracker == null)
                    trackedJoints[vessel].Add(new FlightJointTracker(parentNode, childNodes[i]));
                else if (!trackedJoints[vessel].Contains(existingTracker))
                {
                    trackedJoints[vessel].Add(existingTracker);
                    Vessel currentListing = FindDictEntry(existingTracker);
                    if (currentListing != null)
                        trackedJoints[currentListing].Remove(existingTracker);
                }
            }
        }

        public Vessel FindDictEntry(FlightJointTracker jt)
        {
            for (int i = trackedJoints.Count - 1; i >= 0; i--)
            {
                if (trackedJoints.Values.ElementAt(i).Contains(jt))
                    return trackedJoints.Keys.ElementAt(i);
            }
            return null;
        }

        public void ClearJoints(Vessel vessel)
        {
            if (!trackedJoints.ContainsKey(vessel))
            {
                Log.warn("{0} was not in the dictionary.", vessel.name);
                return;
            }
            for (int i = trackedJoints[vessel].Count - 1; i >= 0; i--)
                trackedJoints[vessel][i].Destroy();
            trackedJoints.Remove(vessel);
            _dictChanged = true;
        }

        public void RegenerateJoints(Vessel vessel)
        {
            if (trackedJoints.ContainsKey(vessel))
                ClearJoints(vessel);
            AddNewVesselToDict(vessel);
            foreach (FlightJointTracker joint in ReCouplerUtils.Generate_Flight(vessel))
            {
                trackedJoints[vessel].Add(joint);
            }
        }

        public void RegenerateJoints()
        {
            List<Vessel> vessels = FlightGlobals.VesselsLoaded;
            for (int i = 0; i < vessels.Count; i++)
            {
                RegenerateJoints(vessels[i]);
            }
        }

        public void AddNewVesselToDict(Vessel vessel)
        {
            if (!trackedJoints.ContainsKey(vessel))
            {
                trackedJoints.Add(vessel, new ObservableCollection<FlightJointTracker>());
                _dictChanged = true;
                trackedJoints[vessel].CollectionChanged += FlightReCoupler_CollectionChanged;
            }
        }

        private void FlightReCoupler_CollectionChanged(object sender, EventArgs e)
        {
            _dictChanged = true;
        }

        public class FlightJointTracker : AbstractJointTracker
        {
            public struct Parent
            {
                public Part part;
                public AttachNode node;
                public AttachNode nodeTo;
                public Parent(Part part, AttachNode node, AttachNode nodeTo)
                {
                    this.part = part;
                    this.node = node;
                    this.nodeTo = nodeTo;
                }
            }
            public List<Parent> parents = new List<Parent>();
            public ConfigurableJoint joint = null;
            public List<PartSet> oldCrossfeedSets = new List<PartSet>(2);
            public bool LinkCreated
            {
                get { return joint != null || IsTrackingDockingPorts; }
            }
            public bool IsTrackingDockingPorts { get; private set; } = false;

            Logger log;

            protected List<ModuleDecouple> cachedDecouplers = null;
            private uint[] oldIDs = new uint[2];

            public List<ModuleDecouple> Decouplers
            {
                get
                {
                    if (cachedDecouplers == null)
                    {
                        cachedDecouplers = new List<ModuleDecouple>();
                        for (int i = 0; i < parts.Count; i++)
                        {
                            cachedDecouplers.AddRange(parts[i].FindModulesImplementing<ModuleDecouple>().FindAll(decoupler => decoupler.isOmniDecoupler || decoupler.ExplosiveNode == nodes[i]));
                        }
                    }
                    return cachedDecouplers;
                }
            }

            public FlightJointTracker(ConfigurableJoint joint, AttachNode parentNode, AttachNode childNode, bool link = true, bool isTrackingDockingPorts = false) : base(parentNode, childNode)
            {
                this.joint = joint;
                //log = new Logger("ReCoupler: FlightJointTracker: " + parts[0].name + " and " + parts[1].name);
                this.IsTrackingDockingPorts = isTrackingDockingPorts;
                this.CheckParents();
                if (link)
                    this.CreateLink();
                if (!isTrackingDockingPorts)
                    this.SetNodes();
            }

            public FlightJointTracker(AttachNode parentNode, AttachNode childNode, bool link = true, bool isTrackingDockingPorts = false) : base(parentNode, childNode)
            {
                //log = new Logger("ReCoupler: FlightJointTracker: " + parts[0].name + " and " + parts[1].name + " ");
                this.IsTrackingDockingPorts = isTrackingDockingPorts;
                this.CheckParents();
                if (link)
                    this.CreateLink();
                if (!isTrackingDockingPorts)
                    this.SetNodes();
            }

            public FlightJointTracker(AbstractJointTracker parent, bool link = true, bool isTrackingDockingPorts = false) : base(parent.nodes[0], parent.nodes[1])
            {
                //log = new Logger("ReCoupler: FlightJointTracker: " + parts[0].name + " and " + parts[1].name + " ");
                this.IsTrackingDockingPorts = isTrackingDockingPorts;
                this.CheckParents();
                if (link)
                    this.CreateLink();
                if (!isTrackingDockingPorts)
                    this.SetNodes();
            }

            public override void SetNodes()
            {
                oldIDs = new uint[] { nodes[0].attachedPartId, nodes[1].attachedPartId };
                base.SetNodes();
                nodes[0].attachedPartId = parts[1].flightID;
                nodes[1].attachedPartId = parts[0].flightID;

                this.structNodeMan[0] = SetModuleStructuralNode(nodes[0]);
                this.structNodeMan[1] = SetModuleStructuralNode(nodes[1]);

                GameEvents.onVesselWasModified.Fire(parts[0].vessel);
            }

            public ConfigurableJoint CreateLink()
            {
                if (this.joint != null)
                {
                    Log.warn("This link already has a joint object.");
                    return this.joint;
                }
                if (IsTrackingDockingPorts)
                {
                    this.joint = new ConfigurableJoint();
                    return this.joint;
                }

                try
                {
                    AttachNode parent = this.nodes[0];
                    AttachNode child = this.nodes[1];
                    Part parentPart = this.parts[0];
                    Part childPart = this.parts[1];

                    Log.dbg("Creating joint between {0} and {1}.", parentPart.name, childPart.name);

                    if (parentPart.Rigidbody == null)
                        Log.error("parentPart body is null :o");
                    if (childPart.Rigidbody == null)
                        Log.error("childPart body is null :o");

                    ConfigurableJoint newJoint;

                    newJoint = childPart.gameObject.AddComponent<ConfigurableJoint>();
                    newJoint.connectedBody = parentPart.Rigidbody;
                    newJoint.anchor = Vector3.zero;                 // There's probably a better anchor point, like the attachNode...
                    newJoint.connectedAnchor = Vector3.zero;

                    newJoint.autoConfigureConnectedAnchor = false;  // Probably don't need.
                    newJoint.axis = Vector3.up;
                    newJoint.secondaryAxis = Vector3.left;
                    newJoint.enableCollision = false;               // Probably don't need.

                    newJoint.breakForce = float.PositiveInfinity;   //Math.Min(parentPart.breakingForce, childPart.breakingForce);
                    newJoint.breakTorque = float.PositiveInfinity;  //Math.Min(parentPart.breakingTorque, childPart.breakingTorque);

                    newJoint.angularXMotion = ConfigurableJointMotion.Limited;
                    newJoint.angularYMotion = ConfigurableJointMotion.Limited;
                    newJoint.angularZMotion = ConfigurableJointMotion.Limited;

                    JointDrive linearDrive = new JointDrive
                    {
                        maximumForce = 1E20f,
                        positionDamper = 0,
                        positionSpring = 1E20f
                    };
                    newJoint.xDrive = linearDrive;
                    newJoint.yDrive = linearDrive;
                    newJoint.zDrive = linearDrive;

                    newJoint.projectionDistance = 0.1f;
                    newJoint.projectionAngle = 180;
                    newJoint.projectionMode = JointProjectionMode.None;

                    newJoint.rotationDriveMode = RotationDriveMode.XYAndZ;
                    newJoint.swapBodies = false;
                    newJoint.targetAngularVelocity = Vector3.zero;
                    newJoint.targetPosition = Vector3.zero;
                    newJoint.targetVelocity = Vector3.zero;
                    newJoint.targetRotation = new Quaternion(0, 0, 0, 1);

                    JointDrive angularDrive = new JointDrive
                    {
                        maximumForce = 1E20f,
                        positionSpring = 60000,
                        positionDamper = 0
                    };
                    newJoint.angularXDrive = angularDrive;
                    newJoint.angularYZDrive = angularDrive;

                    SoftJointLimitSpring zeroSpring = new SoftJointLimitSpring
                    {
                        spring = 0,
                        damper = 0
                    };
                    newJoint.angularXLimitSpring = zeroSpring;
                    newJoint.angularYZLimitSpring = zeroSpring;

                    SoftJointLimit angleSoftLimit = new SoftJointLimit
                    {
                        bounciness = 0,
                        contactDistance = 0,
                        limit = 177
                    };
                    newJoint.angularYLimit = angleSoftLimit;
                    newJoint.angularZLimit = angleSoftLimit;
                    newJoint.highAngularXLimit = angleSoftLimit;
                    newJoint.lowAngularXLimit = angleSoftLimit;

                    this.joint = newJoint;
                }
                catch (Exception ex)
                {
                    Log.error(ex, "Could not create physical joint");
                }
                this.SetNodes();

                ConnectedLivingSpacesCompatibility.RequestAddConnection(this.parts[0], this.parts[1]);
                ReCouplerUtils.onReCouplerJointFormed.Fire(new GameEvents.HostedFromToAction<Vessel, Part>(parts[0].vessel, parts[0], parts[1]));
                return this.joint;
            }

            public static void CombineCrossfeedSets(Part parent, Part child)
            {
                if (parent.crossfeedPartSet == null)
                    return;
                if (parent.crossfeedPartSet.ContainsPart(child))
                    return;

                //log.debug("Combining Crossfeed Sets.");
                HashSet<Part> partsToAdd = parent.crossfeedPartSet.GetParts();
                partsToAdd.UnionWith(child.crossfeedPartSet.GetParts());

                parent.crossfeedPartSet.RebuildParts(partsToAdd);
                child.crossfeedPartSet.RebuildParts(partsToAdd);
                parent.crossfeedPartSet.RebuildInPlace();
                child.crossfeedPartSet.RebuildInPlace();
            }

            public void CombineCrossfeedSets()
            {
                if (IsTrackingDockingPorts)
                    return;
                if (parts[0].crossfeedPartSet == null)
                    return;
                if (parts[0].crossfeedPartSet.ContainsPart(parts[1]))
                    return;
                oldCrossfeedSets.Add(parts[0].crossfeedPartSet);
                oldCrossfeedSets.Add(parts[1].crossfeedPartSet);
                Log.dbg("Part Xfeed: {0} node: {1}", nodes[0].ResourceXFeed, nodes[0].ResourceXFeed);
                Log.dbg("Part Xfeed: {0} node: {1}", nodes[1].ResourceXFeed, nodes[1].ResourceXFeed);
                Log.dbg("Combining Crossfeed Sets.");
                CombineCrossfeedSets(this.parts[0], this.parts[1]);
            }

            public bool Couple(Vessel ownerVessel = null)
            {
                try
                {
                    int owner;
                    if (parts[1].vessel == FlightGlobals.ActiveVessel ||
                        (parts[0].vessel != FlightGlobals.ActiveVessel &&
                            (parts[1].vessel == ownerVessel ||
                            (parts[1].vessel.Parts.Count > parts[0].vessel.Parts.Count && parts[0].vessel != ownerVessel))))
                        owner = 1;
                    else
                        owner = 0;

                    ownerVessel = parts[owner].vessel;

                    AttachNode targetNode = owner == 0 ? nodes[0] : nodes[1];
                    AttachNode coupleNode = owner == 0 ? nodes[1] : nodes[0];
                    Part targetPart = owner == 0 ? parts[0] : parts[1];
                    Part couplePart = owner == 0 ? parts[1] : parts[0];

                    Log.dbg("Coupling {0} to {1}.", couplePart.name, targetPart.name);

                    if (ownerVessel == FlightGlobals.ActiveVessel)
                    {
                        // Save camera information here.
                    }
                    ReCouplerUtils.CoupleParts(coupleNode, targetNode);
                    if(ownerVessel == FlightGlobals.ActiveVessel)
                    {
                        // Restore camera information here
                    }

                    //targetPart.vessel.currentStage = KSP.UI.Screens.StageManager.RecalculateVesselStaging(targetPart.vessel);
                    ownerVessel.currentStage = KSP.UI.Screens.StageManager.RecalculateVesselStaging(ownerVessel) + 1;

                    if (!IsTrackingDockingPorts)
                        this.Destroy();
                    return true;
                }
                catch (Exception ex)
                {
                    Log.error(ex, "Error in coupling");
                    return false;
                }
            }

            public override void Destroy()
            {
                if (IsTrackingDockingPorts)
                    return;

                Log.dbg("Destroying a link.");
                if (joint != null)
                    GameObject.Destroy(joint);

                base.Destroy();

                if (oldCrossfeedSets.Count > 0)
                {
                    parts[0].crossfeedPartSet = oldCrossfeedSets[0];
                    parts[1].crossfeedPartSet = oldCrossfeedSets[1];
                }

                UnsetModuleStructuralNode(nodes[0], structNodeMan[0]);
                UnsetModuleStructuralNode(nodes[1], structNodeMan[1]);

                if (this.parts[0] != null && this.parts[1] != null)
                    ConnectedLivingSpacesCompatibility.RequestRemoveConnection(this.parts[0], this.parts[1]);

                if (this.parts[0] != null)
                    ReCouplerUtils.onReCouplerJointBroken.Fire(new GameEvents.HostedFromToAction<Vessel, Part>(this.parts[0].vessel, this.parts[0], this.parts[1]));
                else if (this.parts[1] != null)
                    ReCouplerUtils.onReCouplerJointBroken.Fire(new GameEvents.HostedFromToAction<Vessel, Part>(this.parts[1].vessel, this.parts[0], this.parts[1]));
                else
                    ReCouplerUtils.onReCouplerJointBroken.Fire(new GameEvents.HostedFromToAction<Vessel, Part>(null, this.parts[0], this.parts[1]));

                if (parts[0] != null && parts[0].vessel != null)
                    GameEvents.onVesselWasModified.Fire(parts[0].vessel);
                else if (parts[1] != null && parts[1].vessel != null)
                    GameEvents.onVesselWasModified.Fire(parts[1].vessel);
            }

            private void CheckParents()
            {
                parents.Capacity = parts.Count;
                for (int i = 0; i < parts.Count; i++)
                {
                    if (parts[i].parent != null)
                    {
                        parents.Add(new Parent(parts[i].parent, parts[i].parent.FindAttachNodeByPart(parts[i]), parts[i].FindAttachNodeByPart(parts[i].parent)));
                    }
                }
            }

            public void PostDecouplerCheck()
            {
                for(int i = parts.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        if (parts[i].vessel != parents[i].part.vessel && ParentStillValid(parents[i]))
                        {
                            Log.dbg("Coupling part to parent.");
                            ReCouplerUtils.CoupleParts(parts[i], parents[i].part, parents[i].nodeTo, parents[i].node);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.error(ex, "Error in PostDecouplerCheck");
                    }
                }
            }

            public bool ParentStillValid(int i) { return ParentStillValid(parents[i]); }

            public static bool ParentStillValid(Parent parent)
            {
                if (parent.node == null)
                    return true;

                if (parent.part.FindModulesImplementing<ModuleDecouple>().Any(
                    decoupler => decoupler.isDecoupled && (decoupler.ExplosiveNode == parent.node || decoupler.isOmniDecoupler)))
                    return false;
                if (parent.nodeTo.owner.FindModulesImplementing<ModuleDecouple>().Any(
                    decoupler => decoupler.isDecoupled && (decoupler.ExplosiveNode == parent.nodeTo || decoupler.isOmniDecoupler)))
                    return false;
                    
                // TODO:
                //if (parent.part.FindModulesImplementing<ModuleDockingNode>().Any(dockingNode => dockingNode.referenceNode == parent.node))
                //    return false;
                return true;
            }
        }
    }
}
