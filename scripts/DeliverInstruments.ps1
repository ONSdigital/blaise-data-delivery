###############################
# Data delivery pipeline script
###############################

# If a serverpark is specified then limit the call to that server park
$catiInstrumentsUri = if([string]::IsNullOrEmpty($env:ServerParkName)) {"$env:ENV_RESTAPI_URL/api/v1/cati/instruments"} 
                      else {"$env:ENV_RESTAPI_URL/api/v1/cati/serverparks/$($env:ServerParkName)/instruments"}

# Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
$instruments = Invoke-RestMethod -Method Get -Uri $catiInstrumentsUri | where { $_.DeliverData -eq $true -and $_.name.StartsWith($env:SurveyType) }

# No active instruments found in CATI
If ($instruments.Count -eq 0) {
    Write-Host "No active instruments found for delivery"
}

# Deliver the instrument package with data for each active instrument
foreach ($instrument in $instruments)
{
    # Build uri to retrive instrument package file with data
    $InstrumentDataUri = "$($env:ENV_RESTAPI_URL)/api/v1/serverparks/$($instrument.serverParkName)/instruments/$($instrument.name)/data"
    
    # Build data delivery filename for the instrument
    $currentDateTime = (Get-Date)
    $fileName = "$($instrument.name).$env:PackageExtension"

    # Download instrument packagegit pu
    wget $InstrumentDataUri -outfile $fileName 
    Write-Host "Downloaded instrument '$($fileName)'"

    # Generate and add SPSS files
    # & .\scripts\AddSpssFilesToInstrument.ps1 "$($instrument.name)"
    # Write-Host "Added SPSS files to instrument"

    # Generate DD filename
    $dataDeliveryFileName = "dd_$($instrument.name)_$($currentDateTime.ToString("ddMMyyyy"))_$($currentDateTime.ToString("HHmmss")).$env:PackageExtension";
    Rename-Item -Path $fileName -NewName $dataDeliveryFileName
    Write-Host "Renamed instrument to '$($dataDeliveryFileName)'"

    # Upload instrument package to NIFI
    gsutil cp $dataDeliveryFileName gs://$env:ENV_BLAISE_NIFI_BUCKET
    Write-Host "Pushed instrument '$($dataDeliveryFileName)' to the NIFI bucket"
    
    # remove local instrument package
    Remove-Item $dataDeliveryFileName
}