/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReCoupler
{
    public abstract class AbstractJointTracker
    {
        public List<Part> parts;
        public List<AttachNode> nodes;
        protected bool[] structNodeMan = new bool[2] { false, false };

        public AbstractJointTracker(AttachNode parentNode, AttachNode childNode)
        {
            this.nodes = new List<AttachNode> { parentNode, childNode };
            this.parts = new List<Part> { nodes[0].owner, nodes[1].owner };
        }

        public virtual void SetNodes()
        {
            nodes[0].attachedPart = parts[1];
            nodes[1].attachedPart = parts[0];
        }

        public virtual void Destroy()
        {
            if (nodes[0] != null)
                nodes[0].attachedPart = null;
            if (nodes[1] != null)
                nodes[1].attachedPart = null;
        }

        protected static bool SetModuleStructuralNode(AttachNode node)
        {
            bool structNodeMan = false;
            ModuleStructuralNode structuralNode = node.owner.FindModulesImplementing<ModuleStructuralNode>().FirstOrDefault(msn => msn.attachNodeNames.Equals(node.id));
            if (structuralNode != null)
            {
                structNodeMan = structuralNode.spawnManually;
                structuralNode.spawnManually = true;
                structuralNode.SpawnStructure();
            }
            return structNodeMan;
        }

        protected static void UnsetModuleStructuralNode(AttachNode node, bool structNodeMan)
        {
            if (node == null)
                return;
            ModuleStructuralNode structuralNode = node.owner.FindModulesImplementing<ModuleStructuralNode>().FirstOrDefault(msn => msn.attachNodeNames.Equals(node.id));
            if (structuralNode != null)
            {
                structuralNode.DespawnStructure();
                structuralNode.spawnManually = structNodeMan;
            }
        }
    }
}
