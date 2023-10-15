param (
    [string]$targetDirectory
)

write-host "MollyCoddle Wrapper >> Calling Molly."



if (-not $targetDirectrory -or $targetDirectory -eq "") {
  write-host "MollyCoddle, you must pass a target directory."
} else {
#$mc = "C:\Files\OneDrive\Dev\Tools\Mollycoddle\mollycoddle.exe"
$mc = "C:\Files\Code\git\mollycoddle\src\mollycoddle\bin\Debug\net7.0\mollycoddle.exe"

&$mc "$targetDirectory" -rulesfile="C:\Files\Code\git\mollycoddle\src\_Dependencies\RulesFiles\default\defaultrules.mollyset" -primaryRoot="C:\Files\OneDrive\Dev\PrimaryFiles" -formatter="plain" -version=default
}