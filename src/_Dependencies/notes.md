tfx extension publish --manifest-globs your-manifest.json --share-with yourOrganization

Update version number then:
tfx extension create --manifest-globs vss-extension.json




### Project Strucutre

Validators describe the rules, multiple rules are loaded into the validators.  
StructureCheckers are collections of CheckEntities.  StructureCheckers are the things that perform the actual checks.
CheckEntities are the things that perform the check as part of a structure checker, each sturcture checker uses multiple checkentities


Checkers use the validators to run the validations against every file that is checked.

.\mollycoddle.exe 

&$mc "$targetDirectory" -rulesfile="C:\Files\Code\git\mollycoddle\src\_Dependencies\RulesFiles\default\defaultrules.mollyset" -primaryRoot="C:\Files\OneDrive\Dev\PrimaryFiles" -formatter="plain" -version=default

.\mollycoddle.exe 'X:\_scratch\tt' '-rulesfile=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/molly/XXVERSIONNAMEXX/defaultrules.mollyset' '-primaryRoot=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/primaryfiles/XXVERSIONNAMEXX/' 



 .\mollycoddle.exe 'X:\_scratch\tt' '-rulesfile=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/molly/default/defaultrules.mollyset' '-primaryRoot=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/primaryfiles/default/' '-get'


.\mollycoddle.exe 'X:\_scratch\tt' '-rulesfile=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/molly/XXVERSIONNAMEXX/defaultrules.mollyset' '-primaryRoot=%NEXUSCONFIG%[R::plisky[L::https://pliskynexus.yellowwater-365987e0.uksouth.azurecontainerapps.io/repository/plisky/primaryfiles/XXVERSIONNAMEXX/' -version=default -formatter=azdo