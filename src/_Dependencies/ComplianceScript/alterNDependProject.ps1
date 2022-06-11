param(
    [Parameter()][string]$nDependFileToAlter = "D:\Scratch\aaDeleteMe\TEMPx.ndproj"
)

$xmlData = new-object xml
$xmlData.Load($nDependFileToAlter);
$queryData = $xmlData.SelectNodes("/NDepend/Queries");
$queryData.RemoveAll();

$xmlData.Save($nDependFileToAlter+".2");
