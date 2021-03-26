###############################
# Data delivery pipeline script
###############################

. "..\functions\LoggingFunctions.ps1"
. "..\functions\FileFunctions.ps1"
. "..\functions\RestApiFunctions.ps1"
. "..\functions\SpssFunctions.ps1"
. "..\functions\DataDeliveryStatusFunctions.ps1"

try {
    # Generating batch stamp for all instruments in the current run to be grouped together
    $batchStamp = GenerateBatchFileName

    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $instruments = GetListOfInstrumentsBySurveyType

    # No active instruments found in CATI
    If ($instruments.Count -eq 0) {
        LogWarning("No instruments found for '$env:SurveyType'")
        exit
    }    

    # Deliver the instrument package with data for each active instrument
    foreach ($instrument in $instruments)
    {
        try {
            
            if($instrument.DeliverData -eq $false)
            {
                CreateDataDeliveryStatus -fileName $instrument.name -state "inactive" -batchStamp $batchStamp
                break
            }
            
            # Generate unique data delivery filename for the instrument
            $deliveryFile = GenerateDeliveryFilename -prefix "dd" -instrumentName $instrument.name

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFile -state "started" -batchStamp $batchStamp

            # Download instrument package
            DownloadInstrumentPackage -serverParkName $instrument.serverParkName -instrumentName $instrument.name -fileName $deliveryFile

            # Generate and add SPSS files
            AddSpssFilesToInstrumentPackage -instrumentPackage $deliveryFile -instrumentName $instrument.name 
        
            # Upload instrument package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $env:ENV_BLAISE_NIFI_BUCKET

            # Set data delivery status to generated
            UpdateDataDeliveryStatus -fileName $deliveryFile -state "generated"

            # Remove local instrument package
            DeleteFile -filePath $deliveryFile
        }
        catch {
            LogError($_.ScriptStackTrace)
            ErrorDataDeliveryStatus -fileName $deliveryFile -state "errored" -error_message "An error has occured in delivering $deliveryFile"
        }
    }
}
catch {
    LogError($_.ScriptStackTrace)
    exit 1
}
