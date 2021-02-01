﻿###############################
# Data delivery pipeline script
###############################

# If a serverpark is specified then limit the call to that server park
$catiInstrumentsUri = if([string]::IsNullOrEmpty($env:ENV_SERVER_PARK)) {"$env:ENV_RESTAPI_URL/cati/instruments"} 
                      else {"$env:ENV_RESTAPI_URL/cati/serverparks/$($env:ENV_SERVER_PARK)/instruments"}

# Retrieve a list of active instruments in CATI for a particular survey type I.E OPN
$instruments = Invoke-RestMethod -Method Get -Uri $catiInstrumentsUri | where { $_.active -eq $false -and $_.name.StartsWith($env:SURVEY_TYPE) }

# No active instruments found in CATI
If ($instruments.Count -eq 0) {
    throw [System.Exception] "$No active instruments found for delivery"
}

# Deliver the instrument package with data for each active instrument
foreach ($instrument in $instruments)
{
    # Build uri to retrive instrument package file with data
    $InstrumentDataUri = "$($env:ENV_RESTAPI_URL)/serverparks/$($instrument.serverParkName)/instruments/$($instrument.name)/data"
    
    # Build data delivery filename for the instrument
    $fileName = "dd_$($instrument.name)_$((Get-Date).ToString("ddMMyyyy"))_$((Get-Date).ToString("HHmmss")).$env:PACKAGE_EXTENSION)";

    # Download instrument package
    wget $InstrumentDataUri -outfile $fileName 
    
    # Upload instrument package to NIFI
    gsutil cp $fileName gs://$env:BLAISE_NIFI_BUCKET
    
    # remove local instrument package
    Remove-Item $fileName
}