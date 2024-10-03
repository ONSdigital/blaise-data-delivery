###############################
# Data delivery pipeline script
###############################

. "$PSScriptRoot\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\functions\FileFunctions.ps1"
. "$PSScriptRoot\functions\RestApiFunctions.ps1"
. "$PSScriptRoot\functions\ThreadingFunctions.ps1"
. "$PSScriptRoot\functions\ConfigFunctions.ps1"

    #Make all errors terminating
    $ErrorActionPreference = "Stop"

try {
    $ddsUrl = $env:ENV_DDS_URL
    LogInfo("DDS URL: $ddsUrl")
    $ddsClientID = $env:ENV_DDS_CLIENT
    LogInfo("DDS Client ID: $ddsClientID")
    $tempPath = $env:TempPath
    LogInfo("Temp path: $tempPath")
    $nifiBucket = $env:ENV_BLAISE_NIFI_BUCKET
    LogInfo("NiFi Bucket: $nifiBucket")
    $dqsBucket = $env:ENV_BLAISE_DQS_BUCKET
    LogInfo("DQS Bucket: $dqsBucket")
    $restAPIUrl = $env:ENV_RESTAPI_URL
    LogInfo("REST API URL: $restAPIUrl")
    $serverParkName = $env:ENV_BLAISE_SERVER_PARK_NAME
    LogInfo("Server park name: $ServerParkName")
    $surveyType = $env:SurveyType
    LogInfo("Survey type: $surveyType")
    $questionnaireList = $env:Questionnaires.Trim() # Trim as we add a shitespace to make this field optional in Azure
    LogInfo("Questionnaire list: $questionnaireList")


    if ([string]::IsNullOrWhitespace($questionnaireList)) {
        # No questionnaires provided so retrieve a list of questionnaires for a particular survey type I.E OPN
        $questionnaires = GetListOfQuestionnairesBySurveyType -restApiBaseUrl $restAPIUrl -surveyType $surveyType -serverParkName $serverParkName
        $questionnaires = $questionnaires | Where-Object { $_.Name -ne "IPS_ContactInfo" } # Filter out IPS_ContactInfo
        LogInfo("Retrieved list of questionnaires for survey type '$surveyType': $($questionnaires | Select-Object -ExpandProperty name)") 
    }
    else {
        # List of questionnaires provided so retrieve a list of questionnaires specified
        $questionnaire_names = $questionnaireList.Split(",")
        LogInfo("Received a list of required questionnaires from pipeline '$questionnaire_names'")
        $questionnaires = GetListOfQuestionnairesByNames -restApiBaseUrl $restAPIUrl -serverParkName $serverParkName -questionnaire_names $questionnaire_names
        $questionnaires = $questionnaires | Where-Object { $_.Name -ne "IPS_ContactInfo" } # Filter out IPS_ContactInfo
        LogInfo("Retrieved list of questionnaires specified $($questionnaires | Select-Object -ExpandProperty name)")
    }

    # No questionnaires found/supplied
    If ($questionnaires.Count -eq 0) {
        LogWarning("No questionnaires found for '$surveyType' on server park '$serverParkName' or supplied via the pipeline")
        exit
    }

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType

    # Generating batch stamp for all questionnaires in the current run to be grouped together
    $batchStamp = GenerateBatchFileName -surveyType $surveyType

    $sync = CreateQuestionnaireSync -questionnaires $questionnaires

    # Deliver the questionnaire package with data for each active questionnaire
    $questionnaires | ForEach-Object -ThrottleLimit $config.throttleLimit -Parallel {
        . "$using:PSScriptRoot\functions\ThreadingFunctions.ps1"

        $process = GetProcess -questionnaire $_ -sync $using:sync

        try {
            . "$using:PSScriptRoot\functions\LoggingFunctions.ps1"
            . "$using:PSScriptRoot\functions\FileFunctions.ps1"
            . "$using:PSScriptRoot\functions\DataDeliveryStatusFunctions.ps1"
            . "$using:PSScriptRoot\functions\RestApiFunctions.ps1"
            . "$using:PSScriptRoot\functions\DeliveryFunctions.ps1"

            # Generate unique data delivery filename for the questionnaire
            LogInfo("Generate unique data delivery filename for questionnaire $($_.name)")
            $deliveryFileName = GenerateDeliveryFilename -prefix "dd" -questionnaireName $_.name -fileExt $using:config.packageExtension

            # Generate full file path for questionnaire
            $deliveryFile = "$tempPath\$deliveryFileName"

            # Set data delivery status to started
            LogInfo("Set data delivery status to started for questionnaire $($_.name)")
            CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "started" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID

            # Create delivery file
            LogInfo("Create delivery file for questionnaire $($_.name)")
            CreateDeliveryFile -deliveryFile $deliveryFile -serverParkName $using:serverParkName -surveyType $using:surveyType -questionnaireName $_.name -dqsBucket $using:dqsBucket -subFolder $processingSubFolder -tempPath $using:tempPath -uneditedData $false          
                     
            # Upload questionnaire package to NIFI
            LogInfo("Upload questionnaire package to NIFI for questionnaire $($_.name)")
            UploadFileToBucket -filePath $deliveryFile -bucketName $using:nifiBucket -deliveryFileName $deliveryFileName

            # Set data delivery status to generated
            LogInfo("Set data delivery status to generated for questionnaire $($_.name)")
            UpdateDataDeliveryStatus -fileName $deliveryFileName -state "generated" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            $process.Status = "Completed"
        }
        catch {
            LogError("Error occured inside: $($_.Exception.Message) at: $($_.ScriptStackTrace)")
            Get-Error
            ErrorDataDeliveryStatus -fileName $deliveryFileName -state "errored" -error_info "An error has occured in delivering $deliveryFileName" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            $process.Status = "Errored"
        }
    }
}
catch {
    LogError("Error occured outside: $($_.Exception.Message) at: $($_.ScriptStackTrace)")
    Get-Error
    exit 1
}

CheckSyncStatus -sync $sync
