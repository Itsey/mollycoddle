write-host "Automated Versioning Active."

$pt = "C:\Files\OneDrive\Dev\Tools\PliskyTool\PliskyTool.exe"
$pt UpdateFiles -Root=c:\files\code\git\mollycoddle -VS="\\unicorn\files\BuildTools\VersionStore\mollycoddle.vstore" -Increment -MM=.\_Dependencies\Automation\AutoverXsion.txt -Debug