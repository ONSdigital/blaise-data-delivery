function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFolder = "$PSScriptRoot\..\configuration"
    $configFile = "$configFolder\$surveyType.json"

    If (Test-Path $configFile) {
        return ConvertJsonFileToObject($configFile)
    }

    return ConvertJsonFileToObject("$configFolder\default.json")
}

function ConvertJsonFileToObject {
    param (
        [string] $configFile
    )  

    return Get-Content -Path $configFile | ConvertFrom-Json
}