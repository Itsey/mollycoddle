Plisky.MollyCoddle Change Log. 

VXXX-VERSION3-XXX
*

V1.4.3
* 🐞 Issue#2 - Incorrect version comparison meant that checking for versions of nuget packages was not reliable.  A bug in the underlying version comparison meant that v1.1.0 would appear greater than 2.0.1.  This is a behaviour change so potentially breaking for some users.
* 🐞 Paging added to the Nexus capabilities and cache altered so that it is now not cleared on start-up and uses an index to determine whether or not to download files.  Aim at reducing chances of MollyCoddle clashing when being run in parallel (e.g. on build machines).

V1.0.3
  * Added multi-targeting for net 8,9,10.
  * 🐞 Fix for Issue raised on Github.  Added retry code on rule read and cache update, and additionally cache only updates on changed file.

V1.0.1
  * Moved to a tool package.  
  * Restructured repository to make it simpler to find rules files.

V0.1.4 
  * Added support for Nexus to replace File Stores.

V0.1.2
  * Altered default terminology.
  * Basic support for Mollycoddle

V0.0.1
* Initial version.

See [http://itsey.github.io](http://itsey.github.io) for more information on the Plisky tools.

