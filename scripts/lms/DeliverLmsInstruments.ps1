###############################
# LMS Data delivery pipeline script
###############################

. "$PSScriptRoot\..\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\..\functions\FileFunctions.ps1"
. "$PSScriptRoot\..\functions\RestApiFunctions.ps1"
. "$PSScriptRoot\..\functions\Threading.ps1"

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

    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $instruments = GetListOfInstrumentsBySurveyType -restApiBaseUrl $restAPIUrl -surveyType $surveyType

    # No active instruments found in CATI
    If ($instruments.Count -eq 0) {
        LogWarning("No instruments found for '$env:SurveyType'")
        exit
    }

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
                CreateDataDeliveryStatus -fileName $deliveryFileName -state "inactive" -batchStamp $using:batchStamp -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
                continue
            }

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFileName -state "started" -batchStamp $using:batchStamp -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID

            # Generate full file path for instrument
            $deliveryFile = "$using:tempPath\$deliveryFileName"

            # Download instrument package
            DownloadInstrumentPackage -restApiBaseUrl $using:restAPIUrl -serverParkName $_.serverParkName -instrumentName $_.name -fileName $deliveryFile

            # Create a temporary folder for processing instruments
            $processingFolder = CreateANewFolder -folderPath $using:tempPath -folderName "$($_.name)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"

            #Gets the folder name of the processing folder
            $processingSubFolderName = GetFolderNameFromAPath -folderPath $processingFolder

            # Create a folder within the temporary folder for generating XML
            $processingSubFolder = CreateANewFolder -folderPath $processingFolder -folderName $processingSubFolderName

            #Add manipula and instrument package to processing folder
            AddManipulaToProcessingFolder -manipulaPackage "$using:tempPath/manipula.zip" -processingFolder $processingFolder -deliveryFile $deliveryFile -tempPath $using:tempPath

            # Generate and add SPSS files
            AddSpssFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $_.name -subFolder $processingSubFolder -dqsBucket $using:dqsBucket -tempPath $using:tempPath

            # Generate and add Ascii files
            AddAsciiFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath

            #Generate XML Files
            AddXMLFileForDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryFile -instrumentName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath

            #Generate Json Files
            AddJSONFileForDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryFile -instrumentName $_.name -subFolder $processingSubFolder -tempPath $using:tempPath

            # Upload instrument package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $using:nifiBucket -deliveryFileName $deliveryFileName

            # Set data delivery status to generated
            UpdateDataDeliveryStatus -fileName $deliveryFileName -state "generated" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            $process.Status = "Completed"

            Remove-Item -Recurse -Force $processingFolder

        }
        catch {
            LogError($_.Exception.Message)
            Get-Error
            ErrorDataDeliveryStatus -fileName $deliveryFileName -state "errored" -error_info "An error has occured in delivering $deliveryFileName" -ddsUrl $using:ddsUrl -ddsClientID $using:ddsClientID
            $process.Status = "Errored"
        }
    }
}
catch {
    LogError($_.Exception.Message)
    Get-Error
    exit 1
}

CheckSyncStatus -sync $sync
