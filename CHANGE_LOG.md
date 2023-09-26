# ReCoupler /L Unleashed :: Change Log

* 2023-0926: 1.3.6.1 (LisiasT) for KSP >= 1.4
	+ Makes it compatible down to KSP 1.4.0
		- Tested only on KSP 1.4.3, but it should work allright on previous ones. 
	+ Moves the thing into my `net.lisias.ksp` hierarchy.
	+ Uses KSPe facilities
		- Log
		- UI
		- File System  
* 2022-0814: 1.3.6 (DBooots) for KSP 1.12.3
	+ Allows ReCoupler paths to cross inline docking nodes if they're extended and active.
	+ Thanks @taniwha for the pull request.
* 2020-1230: 1.3.5 (DBooots) for KSP 1.11.0
	+ Adds options to allow joints between parts separated by a robotic servo or KAS joint.
	+ Changes GUI to use the PopupDialog system.
* 2020-0311: 1.3.4 (DBooots) for KSP 1.9.1
	+ Update for new Unity version and .Net 4.6.1.
* 2019-0622: 1.3.3 (DBooots) for KSP 1.7.2
	+ Recompiled for KSP 1.7.2.
	+ Add compatibility for Breaking Ground Expansion robotics and joints (ReCoupler won't connect parts that have a joint between them).
* 2019-0626: 1.3.3c (DBooots) for KSP 1.7.2
	+ Removed extraneous file thumbs.db. Closing issue #11 from GitHub. No functional changes.
* 2018-1222: 1.3.2 (DBooots) for KSP 1.6.1
	+ Recompiled for KSP 1.6.
* 2018-0326: 1.3.1 (DBooots) for KSP 1.4.4
	+ Removed custom CLS plugin as the official one now supports the interfaces required for ReCoupler and CLS to work together.
* 2017-0510: 1.3.0 (DBooots) for KSP 1.4.1
	+ 1.3.0:
		- Add support for Infernal Robotics (detect joints between potential ReCoupler part pairs and do not join them if they are going to be moving relative to each other)
		- Make user-ignored part pairs persistent.
		- Make the icon much nicer - Credit to @Rodger!
* 2017-0331: 1.2.1 (DBooots) for KSP 1.2.2
	+ 1.2.1:
		- Fix double-jointing of ReCoupler joints (a dev idea partially snuck through)
	+ 1.2.0:
			- Added support for Blizzy's Toolbar.
			- Fixed crossfeed not showing up in the editor.
			- Refactored to increase robustness and remove the need for a VesselModule.
			- All identifying of ReCoupler joints is done in the editor, eliminating the possibility of unwanted joints generating in flight.
			- All saving of ReCoupler joints is handled natively by KSP through sneaky trickery.
			- Bug fixes and efficiency improvement.
* 2017-0331: 1.2.0 (DBooots) for KSP 1.2.2
	+ 1.2.0:
		- Added support for Blizzy's Toolbar.
		- Fixed crossfeed not showing up in the editor.
		- Refactored to increase robustness and remove the need for a VesselModule.
		- All identifying of ReCoupler joints is done in the editor, eliminating the possibility of unwanted joints generating in flight.
		- All saving of ReCoupler joints is handled natively by KSP through sneaky trickery.
		- Bug fixes and efficiency improvement.
* 2017-0328: 1.1.0 (DBooots) for KSP 1.2.2
	+ 1.1.0:
		- Fix virtual joints being forgotten on lower stages.
		- Accommodate parts that change their mesh based on attach node status.
		- Refactor a bunch of the in-editor code to accommodate this^. It's either much more or much less robust now. So we'll see.
* 2017-0324: 1.0.0 (DBooots) for KSP 1.2.2
	+ It's the grand release!
	+ Improvements from the pre-release include:
		- Bug fixes (always bug fixes)
		- A GUI for determining which parts have been joined and removing the virtual links between parts if desired
		- Adjustable settings for the range and node angles that are accepted
		- ConnectedLivingSpace Compatibility (requires custom CLS .dll (included) pending merging of pull request and update)
* 2017-0313: 0.7.4 (DBooots) for KSP 1.2.2 PRE-RELEASE
	+ 0.7.4:
		- Fix interstage nodes accidentally connecting.
* 2017-0310: 0.7.3 (DBooots) for KSP 1.2.2 PRE-RELEASE
	+ 0.7.3 hotfix:
		- Fix CTD resulting from connecting two cargo bays in series.
	+ 0.7.2:
			- Includes the fix for docking ports not undocking.
			- Adds a settings file for configuring the allowable distance and angle between the two nodes.
* 2017-0310: 0.7.2 (DBooots) for KSP 1.2.2
	+ Fix docking ports not being able to undock.
	+ Add a settings file that allows the radius and mutual angle limits between the two attachNodes to be set.
* 2017-0307: 0.7.0 (DBooots) for KSP 1.2.2
	+ No changelog provided
