function GetConfigFromFile {
    param (
        [string] $surveyType
    )

    $configFile = "$PSScriptRoot\..\configuration\$($surveyType).json"

    $config = Get-Content -Path $configFile | ConvertFrom-Json

    return $config
}