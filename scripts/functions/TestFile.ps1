
. "$PSScriptRoot\FileFunctions.ps1"

$tempPath = "D:\Opn\Temp"
$instrumentPackage = "D:\Opn\Temp\OPN2102R.zip"
AddFilesToZip -files "$tempPath\*.sps","$tempPath\*.asc","$tempPath\*.fps" -zipFilePath $instrumentPackage