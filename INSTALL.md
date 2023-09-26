# ReCoupler /L Unleashed

A Mod for Kerbal Space Program that fixes the inability to recombine stacks.

[Unleashed](https://ksp.lisias.net/add-ons-unleashed/) fork by Lisias.


## Installation Instructions

To install, place the GameData folder inside your Kerbal Space Program folder:

* **REMOVE ANY OLD VERSIONS OF THE PRODUCT BEFORE INSTALLING**, including any other fork:
	+ Delete `<KSP_ROOT>/GameData/net.lisias.ksp/ReCoupler`
* Extract the package's `GameData/` folder into your KSP's as follows:
	+ `<PACKAGE>/GameData/net.lisias.ksp/ReCoupler` --> `<KSP_ROOT>/GameData/net.lisias.ksp/`
		- Overwrite any preexisting file.
* Install the remaining dependencies
	+ See below on **Dependencies** 

The following file layout must be present after installation:

```
<KSP_ROOT>
	[GameData]
		[ModuleManagerWatchDog]
			...
		[net.lisias.ksp]
			[ReCoupler]
				...
				CHANGE_LOG.md
				NOTICE
				README.md
				ReCoupler.version
		000_KSPe.dll
		001_KSPe.dll
		666_ModuleManagerWatchDog.dll
		ModuleManager.dll
		...
	KSP.log
	PartDatabase.cfg
	...
```


### Dependencies

+ [KSPe](https://github.com/net-lisias-ksp/KSPe/releases)
	+ **Not Included**
* [Module Manager Watch Dog](https://github.com/net-lisias-ksp/ModuleManagerWatchDog/releases) OPTIONAL
	+ **Not Included** 
* Module Manager 3.1.3 or later
	+ **Not Included**
