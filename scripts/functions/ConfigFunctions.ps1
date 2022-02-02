function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFile = "$PSScriptRoot\..\configuration\$surveyType.json"

    return Get-Content -Path $configFile | ConvertFrom-Json
}