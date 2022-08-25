. "$PSScriptRoot\LoggingFunctions.ps1"

function GetListOfQuestionnairesBySurveyType {
    param (
        [string] $restApiBaseUrl,
        [string] $surveyType,
        [string] $serverParkName,
        [string[]] $questionnaires
    )

    $questionnairesUri = "$restApiBaseUrl/api/v2/serverparks/$($serverParkName)/questionnaires"
    $allQuestionnaires = Invoke-RestMethod -Method Get -Uri $questionnairesUri

    LogInfo("Calling $questionnairesUri to get list of questionnaires")

    if($null -ne $questionnaires -And $questionnaires.Length -gt 0)
    {
        return $allQuestionnaires | Where-Object { $_.name -in $questionnaires }
    }
    else {
        # Return a list of questionnaires for a particular survey type I.E OPN
        return $allQuestionnaires | Where-Object { $_.name.StartsWith($surveyType) }
    }
}
