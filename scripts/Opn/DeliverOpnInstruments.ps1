###############################
# Data delivery pipeline script
###############################

try {
    . "$PSScriptRoot\functions\LoggingFunctions.ps1"
    . "$PSScriptRoot\functions\FileFunctions.ps1"
    . "$PSScriptRoot\functions\RestApiFunctions.ps1"
    . "$PSScriptRoot\functions\SpssFunctions.ps1"

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
            $fileName = GenerateUniqueFilename -prefix "dd" -instrumentName $instrument.name

            # Download instrument package
            DownloadInstrumentPackage -serverParkName $instrument.serverParkName -instrumentName $instrument.name -fileName $fileName

            # Generate and add SPSS files
            AddSpssFilesToInstrumentPackage -instrumentPackage $fileName -instrumentName $instrument.name 
        
            # Upload instrument package to NIFI
            UploadFileToBucket -fileName $fileName -bucketName $env:ENV_BLAISE_NIFI_BUCKET

            # remove local instrument package
            Remove-Item $fileName
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
