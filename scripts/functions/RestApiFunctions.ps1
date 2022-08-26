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


function GetQuestionnaires {
    param (
        [string] $restApiBaseUrl,
        [string] $serverParkName,
        [string[]] $questionnaireList
    )

    if($null -eq $questionnaireList -or $questionnaireList.Length -eq 0) {
        LogInfo("No questionnaires provided to retrieve")
        exit
    }

    $questionnairesUri = "$restApiBaseUrl/api/v2/serverparks/$($serverParkName)/questionnaires"
    $allQuestionnaires = Invoke-RestMethod -Method Get -Uri $questionnairesUri

    LogInfo("Calling $questionnairesUri to get list of questionnaires")
    LogInfo("List $questionnaireList")

    return $allQuestionnaires | Where-Object { $_.name -in $questionnaireList }
}
