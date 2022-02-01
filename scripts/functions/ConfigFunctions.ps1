function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFile = "$PSScriptRoot\configuration\$($surveyType).json"

    return Get-Content -Raw -Path $configFile | ConvertFrom-Json
}