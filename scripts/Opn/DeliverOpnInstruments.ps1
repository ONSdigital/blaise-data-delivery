###############################
# Data delivery pipeline script
###############################

. "$PSScriptRoot\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\functions\FileFunctions.ps1"
. "$PSScriptRoot\functions\RestApiFunctions.ps1"
. "$PSScriptRoot\functions\SpssFunctions.ps1"

try {
    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $instruments = GetListOfActiveInstruments

    # No active instruments found in CATI
    If ($instruments.Count -eq 0) {
        LogWarning("No active instruments found for delivery")
    }    

    # Deliver the instrument package with data for each active instrument
    foreach ($instrument in $instruments)
    {
        try {           
            # Generate unique data delivery filename for the instrument
            $deliveryFile = GenerateDeliveryFilename -prefix "dd" -instrumentName $instrument.name

            # Download instrument package
            DownloadInstrumentPackage -serverParkName $instrument.serverParkName -instrumentName $instrument.name -fileName $deliveryFile

            # Generate and add SPSS files
            AddSpssFilesToInstrumentPackage -instrumentPackage $deliveryFile -instrumentName $instrument.name 
        
            # Upload instrument package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $env:ENV_BLAISE_NIFI_BUCKET

            # Remove local instrument package
            DeleteFile -filePath $deliveryFile
        }
        catch {
            LogError($_.ScriptStackTrace)
        }
    }
}
catch {
    LogError($_.ScriptStackTrace)
    exit 1
}
