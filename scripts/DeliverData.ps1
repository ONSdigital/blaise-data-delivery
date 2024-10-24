. "$PSScriptRoot\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\functions\FileFunctions.ps1"
. "$PSScriptRoot\functions\RestApiFunctions.ps1"
. "$PSScriptRoot\functions\ThreadingFunctions.ps1"
. "$PSScriptRoot\functions\ConfigFunctions.ps1"

# Make all errors terminating
$ErrorActionPreference = "Stop"

try {
    $ddsUrl = $env:ENV_DDS_URL
    $ddsClientID = $env:ENV_DDS_CLIENT
    $tempPath = $env:TempPath
    $nifiBucket = $env:ENV_BLAISE_NIFI_BUCKET
    $dqsBucket = $env:ENV_BLAISE_DQS_BUCKET
    $restAPIUrl = $env:ENV_RESTAPI_URL
    $serverParkName = $env:ENV_BLAISE_SERVER_PARK_NAME
    $surveyType = $env:SurveyType
    $questionnaireList = $env:Questionnaires -replace '\s+', '' # Remove all spaces from the questionnaire list

    LogInfo("DDS URL: $ddsUrl")
    LogInfo("DDS Client ID: $ddsClientID")
    LogInfo("Temp path: $tempPath")
    LogInfo("NiFi Bucket: $nifiBucket")
    LogInfo("DQS Bucket: $dqsBucket")
    LogInfo("REST API URL: $restAPIUrl")
    LogInfo("Server park name: $serverParkName")
    LogInfo("Survey type: $surveyType")
    LogInfo("Questionnaire list: $questionnaireList")

    # Get list of questionnaires for data delivery
    $questionnaires = if ([string]::IsNullOrWhitespace($questionnaireList)) {
        LogInfo("Getting installed questionnaires for survey type: '$surveyType'")
        GetListOfQuestionnairesBySurveyType -restApiBaseUrl $restAPIUrl -surveyType $surveyType -serverParkName $serverParkName
    } else {
        $questionnaire_names = $questionnaireList.Split(",")
        LogInfo("Received questionnaires for data delivery from pipeline: '$questionnaire_names'")
        GetListOfQuestionnairesByNames -restApiBaseUrl $restAPIUrl -serverParkName $serverParkName -questionnaire_names $questionnaire_names
    }

    # Filter out questionnaires with "contactinfo" or "attempts" in their name
    $questionnaires = $questionnaires | Where-Object { $_.Name -notmatch "contactinfo|attempts" } # Filter out questionnaires with "contactinfo" or "attempts" in their name
    LogInfo("Questionnaires for data delivery: $($questionnaires | Select-Object -ExpandProperty name)")

    # No questionnaires found/supplied
    If ($questionnaires.Count -eq 0) {
        LogWarning("No questionnaires found for survey type '$surveyType' on server park '$serverParkName' or supplied via the pipeline")
        exit
    }

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType

    # Generate unique batch stamp for grouping all questionnaires processed in the current run
    $batchStamp = GenerateBatchFileName -surveyType $surveyType

    # Create synchronised hashtable to manage the processing status of each questionnaire
    $sync = CreateQuestionnaireSync -questionnaires $questionnaires

    # Deliver the questionnaire package with data for each questionnaire
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
            $deliveryFileName = GenerateDeliveryFilename -prefix "dd" -questionnaireName $_.name -fileExt $using:config.packageExtension

            # Generate full file path for questionnaire
            $deliveryFile = Join-Path $using:tempPath $deliveryFileName

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "started" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID

            # Create delivery file
            CreateDeliveryFile -deliveryFile $deliveryFile -serverParkName $using:serverParkName -surveyType $using:surveyType -questionnaireName $_.name -dqsBucket $using:dqsBucket -subFolder $processingSubFolder -tempPath $using:tempPath -uneditedData $false          
                        
            # Upload questionnaire package to NiFi
            UploadFileToBucket -filePath $deliveryFile -bucketName $using:nifiBucket -deliveryFileName $deliveryFileName

            # Set data delivery status to generated
            UpdateDataDeliveryStatus -fileName $deliveryFileName -state "generated" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            
            $process.Status = "Completed"
        }
        catch {
            LogError("Error occurred: $($_.Exception.Message)")
            LogError("Stack trace: $($_.ScriptStackTrace)")
            ErrorDataDeliveryStatus -fileName $deliveryFileName -state "errored" -error_info "An error has occurred in delivering $deliveryFileName" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            $process.Status = "Errored"
        }
    }
}
catch {
    LogError("Error occurred: $($_.Exception.Message)")
    LogError("Stack trace: $($_.ScriptStackTrace)")
    exit 1
}

CheckSyncStatus -sync $sync
