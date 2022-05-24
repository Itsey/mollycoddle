param(
    [Parameter()][string]$repositoryPath
)

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

write-host "Toolset Compliance Checker, Online."

if ($null -eq $env:TOOLSETRUNCOUNT ) {
write-host "##vso[task.setvariable variable=toolsetRunCount;]1"

.\molly\mollycoddle.exe  $repositoryPath -rulesFile=".\mollyrules\defaultrules.mollyset" -masterRoot="\\blackpearl\DevShare\MasterFiles" -formatter=azdo -warnonly 
 $retcode = $LASTEXITCODE
pop-location
}
exit $retcode