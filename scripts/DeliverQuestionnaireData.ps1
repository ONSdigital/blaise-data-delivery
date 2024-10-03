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

    #get configuration for survey type
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
            . "$using:PSScriptRoot\functions\CloudFunctions.ps1"
            . "$using:PSScriptRoot\functions\SpssFunctions.ps1"
            . "$using:PSScriptRoot\functions\XmlFunctions.ps1"
            . "$using:PSScriptRoot\functions\JsonFunctions.ps1"
            . "$using:PSScriptRoot\functions\AsciiFunctions.ps1"
            . "$using:PSScriptRoot\functions\ManipulaFunctions.ps1"

            # Generate unique data delivery filename for the questionnaire
            $deliveryFileName = GenerateDeliveryFilename -prefix "dd" -questionnaireName $_.name -fileExt $using:config.packageExtension

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "started" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID

            # Generate full file path for questionnaire
            $deliveryFile = "$using:tempPath\$deliveryFileName"

            # Download questionnaire package
            DownloadFileFromBucket -questionnaireFileName "$($_.name).bpkg" -bucketName $using:dqsBucket -filePath $deliveryFile

            # Populate data
            # the use of the parameter '2>&1' redirects output of the cli to the command line and will allow any errors to bubble up
            C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $using:serverParkName -q $_.name -f $deliveryFile -a $using:config.auditTrailData -b $using:config.batchSize 2>&1        
            
            # Create a temporary folder for processing questionnaires
            $processingFolder = CreateANewFolder -folderPath $using:tempPath -folderName "$($_.name)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"

            # If we need to use subfolders then create one and set variable
            if($using:config.createSubFolder -eq $true) {
                LogInfo("Creating subfolder for delivery")

                # Gets the folder name of the processing folder
                $processingSubFolderName = GetFolderNameFromAPath -folderPath $processingFolder

                # Create a sub folder within the temporary folder
                $processingSubFolder = CreateANewFolder -folderPath $processingFolder -folderName $processingSubFolderName
            }
            else {
                # This variable will be ignored in the fucntion called if passed - ugh
                LogInfo("Did not create subfolder for delivery")
                $processingSubFolder = $NULL
            }

            #Add manipula and questionnaire package to processing folder
            LogInfo("Add manipula")
            AddManipulaToProcessingFolder -manipulaPackage "$using:tempPath/manipula.zip" -processingFolder $processingFolder -deliveryFile $deliveryFile -tempPath $using:tempPath

            # Generate and add SPSS files if configured
            if($using:config.deliver.spss -eq $true) {
                LogInfo("Adding SPSS files")
                AddSpssFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -questionnaireName $_.name -dqsBucket $using:dqsBucket -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Generate and add Ascii files if configured
            if($using:config.deliver.ascii -eq $true) {
                LogInfo("Adding ASCII files")
                AddAsciiFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -questionnaireName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Generate and add XML Files if configured
            if($using:config.deliver.xml -eq $true) {
                LogInfo("Adding XML files")
                AddXMLFileToDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryFile -questionnaireName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Generate and add json Files if configured
            if($using:config.deliver.json -eq $true) {
                LogInfo("Adding JSON files")
                AddJSONFileToDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryFile -questionnaireName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Upload questionnaire package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $using:nifiBucket -deliveryFileName $deliveryFileName

            # Set data delivery status to generated
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
