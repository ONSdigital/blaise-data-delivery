###############################
# Data delivery pipeline script
###############################

. "$PSScriptRoot\..\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\..\functions\FileFunctions.ps1"
. "$PSScriptRoot\..\functions\RestApiFunctions.ps1"
. "$PSScriptRoot\..\functions\Threading.ps1"
. "$PSScriptRoot\..\functions\ConfigFunctions.ps1"

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
    $surveyType = $env:SurveyType
    LogInfo("Survey type: $surveyType")
    $packageExtension = $env:PackageExtension
    LogInfo("Package Extension: $packageExtension")
    $serverParkName = $env:ServerParkName
    LogInfo("Server park name: $ServerParkName")

    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $instruments = GetListOfInstrumentsBySurveyType -restApiBaseUrl $restAPIUrl -surveyType $surveyType

    # No active instruments found in CATI
    If ($instruments.Count -eq 0) {
        LogWarning("No instruments found for '$env:SurveyType'")
        exit
    }

    #get configuration for survey type
    $config = GetConfigFromFile -SurveyType $surveyType

    # Generating batch stamp for all instruments in the current run to be grouped together
    $batchStamp = GenerateBatchFileName -SurveyType $surveyType

    $sync = CreateInstrumentSync -instruments $instruments

    # Deliver the instrument package with data for each active instrument
    $instruments | ForEach-Object -ThrottleLimit 3 -Parallel {
        . "$using:PSScriptRoot\..\functions\Threading.ps1"

        $process = GetProcess -instrument $_ -sync $using:sync

        try {
            . "$using:PSScriptRoot\..\functions\LoggingFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\FileFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\DataDeliveryStatusFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\RestApiFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\CloudFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\SpssFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\xmlFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\JsonFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\AsciiFunctions.ps1"
            . "$using:PSScriptRoot\..\functions\ManipulaFunctions.ps1"

            # Generate unique data delivery filename for the instrument
            $deliveryFileName = GenerateDeliveryFilename -prefix "dd" -instrumentName $_.name -fileExt $using:packageExtension

            if ($_.DeliverData -eq $false) {
                CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "inactive" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
                continue
            }

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "started" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID

            # Generate full file path for instrument
            $deliveryFile = "$using:tempPath\$deliveryFileName"

            # Download instrument package
            DownloadFileFromBucket -instrumentFileName "$($_.name).bpkg" -bucketName $using:dqsBucket -filePath $deliveryFile

            # Populate data
            C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $using:serverParkName -i $_.name -f $deliveryFile

            # Create a temporary folder for processing instruments
            $processingFolder = CreateANewFolder -folderPath $using:tempPath -folderName "$($_.name)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"

            # If we need to use subfolders then create one and set variable
            if($config.createSubFolder -eq $true) {
                # Gets the folder name of the processing folder
                $processingSubFolderName = GetFolderNameFromAPath -folderPath $processingFolder

                # Create a sub folder within the temporary folder
                $processingSubFolder = CreateANewFolder -folderPath $processingFolder -folderName $processingSubFolderName
            }
            else {
                # This variable will be ignored in the fucntion called if passed - ugh
                $processingSubFolder = $NULL
            }

            #Add manipula and instrument package to processing folder
            AddManipulaToProcessingFolder -manipulaPackage "$using:tempPath/manipula.zip" -processingFolder $processingFolder -deliveryFile $deliveryFile -tempPath $using:tempPath

            # Generate and add SPSS files if configured
            if($config.deliver.spss -eq $true) {
                AddSpssFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $_.name -dqsBucket $using:dqsBucket -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Generate and add Ascii files if configured
            if($config.deliver.ascii -eq $true) {
                AddAsciiFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Generate and add XML Files if configured
            if($config.deliver.xml -eq $true) {
                AddXMLFileForDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryFile -instrumentName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Generate and add son Files if configured
            if($config.deliver.json -eq $true) {
                AddJSONFileForDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryFile -instrumentName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath
            }

            # Upload instrument package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $using:nifiBucket -deliveryFileName $deliveryFileName

            # Set data delivery status to generated
            UpdateDataDeliveryStatus -fileName $deliveryFileName -state "generated" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            $process.Status = "Completed"

            # TODO check if this happens in pipeline as it is not configured in the OPN script
            Remove-Item -Recurse -Force $processingFolder
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
