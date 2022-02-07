function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFile = "$PSScriptRoot\..\configuration\$surveyType.json"

    If (Test-Path $configFile) {
        return ConvertJsonFileToObject($configFile)
    }

    return ConvertJsonFileToObject("$PSScriptRoot\..\configuration\default.json")
}

function ConvertJsonFileToObject {
    param (
        [string] $configFile
    )  

    return Get-Content -Path $configFile | ConvertFrom-Json
}