param(
    [Parameter()][string]$repositoryPath
)

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

write-host "Toolset Compliance Checker, Online."

# Varibales that change by environment.

$solutionFile = "XXXINVALIDXXX";
$buildToolsDir = "C:\files\BuildTools";
$ndependCommandLine = $buildToolsDir+"\Ndepend\Ndepend.Console.Exe";
$ndependAnalysisRulesFile = $buildToolsDir + "\compliance\ndependrules\activecustomqualityrules.ndrules";
$masterRoot = "\\blackpearl\DevShare\MasterFiles";
$mollyCommandline = ".\molly\mollycoddle.exe";
$tr = "-masterRoot=" + $masterRoot;

$mollyCommandline = ".\molly\mollycoddle.exe";

if ($null -eq $env:SOLUTION) {
    write-host "Solution Not Found, searching for the solution.";    

    $slnName = @(get-childitem -path $repositoryPath -recurse -erroraction SilentlyContinue -include *.sln | select FullName)[0]
    if ($null -eq $slnName ) {
        write-host "WARNING - No Solution File Found"
    } else {
      $solutionFile =  $slnName.FullName
    }

} else {
    $solutionFile = $env:SOLUTION;
}

if ($null -eq $env:TOOLSETRUNCOUNT ) {
  # First run happens immediately after checkout, but before code build and compile

  write-host "##vso[task.setvariable variable=toolsetRunCount;]First Run Active";


 & $mollyCommandline  $repositoryPath -rulesFile=".\mollyrules\defaultrules.mollyset" $tr -formatter=azdo  
 $retcode = $LASTEXITCODE

  if ($retcode -ne 0) {
    write-host "Molly Failure, Breaking Build" 
  }
} else {

  # Second run happens at the end of the script, after all user steps before the post job activities.

  $tempProj = New-TemporaryFile  
  $newNameTempProj = $([System.IO.Path]::ChangeExtension($tempProj, ".ndproj"));
  Rename-Item -Path $tempProj -NewName $newNameTempProj;
  $tempProj = $newNameTempProj;

  write-host "using $tempProj for ndepend analysis";
  & $ndependCommandLine /cp $tempProj $solutionFile`|-_Dependencies

  $xmlData = new-object xml
  $xmlData.Load($tempProj);
  $queryData = $xmlData.SelectNodes("/NDepend/Queries");
  
  if ($queryData.Count -gt 0) {  
      $queryData.RemoveAll();
  }

  $xmlData.Save($tempProj);

  & $ndependCommandLine $tempProj /OutDir $env:BUILD_STAGINGDIRECTORY /DontPersistHistoricAnalysisResult /Concurrent /RuleFiles $ndependAnalysisRulesFile
  $retcode = $LASTEXITCODE
  
  Remove-Item $tempProj
}

pop-location
exit $retcode