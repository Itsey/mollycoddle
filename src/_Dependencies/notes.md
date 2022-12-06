tfx extension publish --manifest-globs your-manifest.json --share-with yourOrganization

Update version number then:
tfx extension create --manifest-globs vss-extension.json




### Project Strucutre

Validators describe the rules, multiple rules are loaded into the validators.  
StructureCheckers are collections of CheckEntities.  StructureCheckers are the things that perform the actual checks.
CheckEntities are the things that perform the check as part of a structure checker, each sturcture checker uses multiple checkentities


Checkers use the validators to run the validations against every file that is checked.

