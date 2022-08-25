. "$PSScriptRoot\LoggingFunctions.ps1"

function GetListOfQuestionnairesBySurveyType {
    param (
        [string] $restApiBaseUrl,
        [string] $surveyType,
        [string] $serverParkName
    )

    $questionnairesUri = "$restApiBaseUrl/api/v2/serverparks/$($serverParkName)/questionnaires"
    $allQuestionnaires = Invoke-RestMethod -Method Get -Uri $questionnairesUri

    LogInfo("Calling $questionnairesUri to get list of questionnaires")
    # Return a list of questionnaires for a particular survey type I.E OPN
    return $allQuestionnaires | Where-Object { $_.name.StartsWith($surveyType) }
}

function GetQuestionnaire {
    param (
        [string] $restApiBaseUrl,
        [string] $questionnaireName,
        [string] $serverParkName
    )

    $questionnairesUri = "$restApiBaseUrl/api/v2/serverparks/$($serverParkName)/questionnaires/$($questionnaireName)"
    $questionnaire = Invoke-RestMethod -Method Get -Uri $questionnairesUri

    LogInfo("Calling $questionnairesUri to get questionnaire $questionnaire")
    # Return questionnaire
    return $questionnaire
}