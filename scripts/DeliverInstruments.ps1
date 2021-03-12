###############################
# Data delivery pipeline script
###############################

try {
    . "$PSScriptRoot\Logging.ps1"

    # If a serverpark is specified then limit the call to that server park
    $catiInstrumentsUri = if([string]::IsNullOrEmpty($env:ServerParkName)) {"$env:ENV_RESTAPI_URL/api/v1/cati/instruments"} 
                        else {"$env:ENV_RESTAPI_URL/api/v1/cati/serverparks/$($env:ServerParkName)/instruments"}

    # Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
    $instruments = Invoke-RestMethod -Method Get -Uri $catiInstrumentsUri | Where-Object { $_.DeliverData -eq $true -and $_.name.StartsWith($env:SurveyType) }

    # No active instruments found in CATI
    If ($instruments.Count -eq 0) {
        LogWarning("No active instruments found for delivery")
    }

    # Deliver the instrument package with data for each active instrument
    foreach ($instrument in $instruments)
    {
        try {
            # Build uri to retrive instrument package file with data
            $InstrumentDataUri = "$($env:ENV_RESTAPI_URL)/api/v1/serverparks/$($instrument.serverParkName)/instruments/$($instrument.name)/data"
            
            # Build data delivery filename for the instrument
            $fileName = "$($instrument.name).$env:PackageExtension"

            # Download instrument packagegit pu
            Invoke-WebRequest $InstrumentDataUri -outfile $fileName 
            LogInfo("Downloaded instrument '$fileName'")

            # Generate and add SPSS files
            & .\scripts\AddSpssFilesToInstrument.ps1 "$($instrument.name)" | Out-Null
            LogInfo("Added SPSS files to instrument")

            # Generate DD filename
            $currentDateTime = (Get-Date)
            $dataDeliveryFileName = "dd_$($instrument.name)_$($currentDateTime.ToString("ddMMyyyy"))_$($currentDateTime.ToString("HHmmss")).$env:PackageExtension";
            Rename-Item -Path $fileName -NewName $dataDeliveryFileName
            LogInfo("Renamed instrument to '$dataDeliveryFileName'")
         
            # Upload instrument package to NIFI
            gsutil cp $dataDeliveryFileName gs://$env:ENV_BLAISE_NIFI_BUCKET
            LogInfo("Pushed instrument '$dataDeliveryFileName' to the NIFI bucket '$($env:ENV_BLAISE_NIFI_BUCKET)'")

            # remove local instrument package
            Remove-Item $dataDeliveryFileName
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
