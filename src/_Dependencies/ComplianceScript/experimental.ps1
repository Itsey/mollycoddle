$slnName = @(get-childitem -path C:\Files\Code\git\mollycoddle\src -recurse -erroraction SilentlyContinue -include *.slnx | select FullName)[0]
if ($null -eq $slnName ) {
    write-host "not found"
} else {
write-host $slnName.FullName
}