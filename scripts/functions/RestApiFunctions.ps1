. "$PSScriptRoot\LoggingFunctions.ps1"

function GetListOfActiveInstruments {
    param (
        [string] $restApiBaseUrl,
        [string] $surveyType
    )

    $catiInstrumentsUri = "$restApiBaseUrl/api/v1/cati/instruments"

    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    return Invoke-RestMethod -Method Get -Uri $catiInstrumentsUri | Where-Object { $_.DeliverData -eq $true -and $_.name.StartsWith($surveyType) }
}

function GetListOfInstrumentsBySurveyType {
    param (
        [string] $restApiBaseUrl,
        [string] $surveyType
    )

    $catiInstrumentsUri = "$restApiBaseUrl/api/v1/cati/instruments"

    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $allInstruments = Invoke-RestMethod -Method Get -Uri $catiInstrumentsUri

    return $allInstruments | Where-Object { $_.name.StartsWith($surveyType) }
}
