function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFile = "$PSScriptRoot\..\configuration\$surveyType.json"

    If (Test-Path $configFile) {
        return Get-Content -Path $configFile | ConvertFrom-Json
    }

    return Get-Content -Path "$PSScriptRoot\..\configuration\default.json" | ConvertFrom-Json
}