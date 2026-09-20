# Plisky.Mollycoddle

## Why

This is a dotnet tool which provides repository linting.  This was originally conceived to track down duff language files sneaking their way into repositories but has grown to have a rules based system for directory structure and file linting. 



## Release Notes

#### VNext

Added in paging support and then added in an index file to use sha1 to determine whether or not the cache needed refreshing and this meant stopping it clearing the cache on startup.  Very likely that the cache clear was also causing the locking issue that is reported.  Lots of code tidyup following the addition of stylecop and roslynator analysers to the main projects.  Not yet fixed the version comparison issue.  Stress test ruleset added with 3000 files to check that the paging and caching was working.  Small update to the error message if the file isn't found to make it clearer what the issue is.

#### 1.0.3 

Added net 10 multi targeting in for net 8,9,10.  Release note for 101 stated that it was present but could not find any evidence of it in the tools package code. 

Removed some null check warnings from the code to reduce warning count, no functional change.

Updated copyright to 26.

Removed `releasenotesfile` element from the project structure because versonify has it but pointing to a non-existent file so assume that this cant be used and instead the release note `msbuild` step works.

🐞Issue#1, LFY-60 - Parallel usage bug addressed.  Assumption is that the bug occurs on cache refresh update, therefore two changes made, first only update the cache if the contents of the file has changed and second place a retry around both the file update and the rules read functionality.  

⚠️Note use of bilge actions capability for unit testing, likely that this is the first time that this is used and left in production code, functionality allows for testing of the retry capability quite nicely though.

⚠️Note dependency on pre-release PNF to allow adding zero return code for build to run through.  Should be corrected next time now that the PNF loop can fix that missing issue.



#### 1.0.1

Moved it to a dotnet tool package.

Note release note used to say multi-targeting added but can find no evidence of that while adding it in for 1.0.2

