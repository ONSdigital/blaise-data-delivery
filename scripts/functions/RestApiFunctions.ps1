. "$PSScriptRoot\LoggingFunctions.ps1"

function GetListOfQuestionnairesBySurveyType {
    param (
        [string] $restApiBaseUrl,
        [string] $surveyType,
        [string] $serverParkName
    )

    LogInfo("Calling $questionnairesUri to get list of questionnaires")
    $questionnairesUri = "$restApiBaseUrl/api/v2/serverparks/$($serverParkName)/questionnaires"
    $allQuestionnaires = Invoke-RestMethod -Method Get -Uri $questionnairesUri


    # Return a list of questionnaires for a particular survey type I.E OPN
    return $allQuestionnaires | Where-Object { $_.name.StartsWith($surveyType) }
}


function GetListOfQuestionnairesByNames {
    param (
        [string] $restApiBaseUrl,
        [string] $serverParkName,
        [string[]] $questionnaire_names
    )

    if($null -eq $questionnaire_names -or $questionnaire_names.Length -eq 0) {
        LogInfo("No questionnaires provided to retrieve")
        exit
    }

    LogInfo("Calling $questionnairesUri to get list of questionnaires")

    $questionnairesUri = "$restApiBaseUrl/api/v2/serverparks/$($serverParkName)/questionnaires"
    $allQuestionnaires = Invoke-RestMethod -Method Get -Uri $questionnairesUri

    return $allQuestionnaires | Where-Object { $_.name -in $questionnaire_names }
}
