# ReCoupler /L Unleashed :: Change Log

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
