. "$PSScriptRoot\functions\FileFunctions.ps1"


$result = GenerateUniqueFilename -prefix "dd" -instrumentName "OPN2101A" -fileExt "zip"
Write-Host $result