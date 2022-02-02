. "$PSScriptRoot\LoggingFunctions.ps1"

function GetListOfInstrumentsBySurveyType {
    param (
        [string] $restApiBaseUrl,
        [string] $surveyType,
        [string] $serverParkName
    )

    $instrumentsUri = "$restApiBaseUrl/api/v1/serverparks/$($serverParkName)/instruments"

    # Retrieve a list of instruments for a particular survey type I.E OPN
    return Invoke-RestMethod -Method Get -Uri $instrumentsUri | Where-Object { $_.name.StartsWith($surveyType) }
}
