. "$PSScriptRoot\FileFunctions.ps1"

function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFolder = "$PSScriptRoot\..\..\configuration"
    $configFile = "$configFolder\$surveyType.json"

    If (Test-Path $configFile) {
        return ConvertJsonFileToObject($configFile)
    }

    return ConvertJsonFileToObject("$configFolder\default.json")
}