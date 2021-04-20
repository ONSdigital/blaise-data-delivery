###############################
# Data delivery pipeline script
###############################

. "$PSScriptRoot\..\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\..\functions\FileFunctions.ps1"
. "$PSScriptRoot\..\functions\RestApiFunctions.ps1"
. "$PSScriptRoot\..\functions\CloudFunctions.ps1"
. "$PSScriptRoot\..\functions\SpssFunctions.ps1"
. "$PSScriptRoot\..\functions\DataDeliveryStatusFunctions.ps1"
. "$PSScriptRoot\..\functions\ManipulaFunctions.ps1"

try {
    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $instruments = GetListOfInstrumentsBySurveyType

    # No active instruments found in CATI
    If ($instruments.Count -eq 0) {
        LogWarning("No instruments found for '$env:SurveyType'")
        exit
    }    

    # Generating batch stamp for all instruments in the current run to be grouped together
    $batchStamp = GenerateBatchFileName

    # Deliver the instrument package with data for each active instrument
    foreach ($instrument in $instruments)
    {
        try {
            
            # Generate unique data delivery filename for the instrument
            $deliveryFileName = GenerateDeliveryFilename -prefix "dd" -instrumentName $instrument.name
            
            if($instrument.DeliverData -eq $false)
            {
                CreateDataDeliveryStatus -fileName $deliveryFileName -state "inactive" -batchStamp $batchStamp
                continue
            }

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFileName -state "started" -batchStamp $batchStamp

            # Generate full file path for instrument
            $deliveryFile = "$env:TempPath\$deliveryFileName"

            # Download instrument package
            DownloadInstrumentPackage -serverParkName $instrument.serverParkName -instrumentName $instrument.name -fileName $deliveryFile

            # Create a temporary folder for processing instruments
            $processingFolder = CreateANewFolder -folderPath $env:TempPath -folderName "$($instrument.name)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"

            #Add manipula and instrument package to processing folder
            AddManipulaToProcessingFolder -processingFolder $processingFolder -deliveryFile $deliveryFile

            # Generate and add SPSS files
            AddSpssFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $instrument.name 
        
            # Upload instrument package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $env:ENV_BLAISE_NIFI_BUCKET

            # Set data delivery status to generated
            UpdateDataDeliveryStatus -fileName $deliveryFileName -state "generated"
        }
        catch {
            LogError("Error occured: $_.Exception.Message at: $_.ScriptStackTrace")
            ErrorDataDeliveryStatus -fileName $deliveryFileName -state "errored" -error_info "An error has occured in delivering $deliveryFileName"
        }
    }
}
catch {
    LogError("Error occured: $_.Exception.Message at: $_.ScriptStackTrace")
    exit 1
}
