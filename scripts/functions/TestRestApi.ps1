
. "$PSScriptRoot\RestApiFunctions.ps1"
$instruments = GetListOfActiveInstruments -restApiBaseUrl "http://localhost:90" -surveyType "OPN"
Write-Host "$($instruments.Count)"


