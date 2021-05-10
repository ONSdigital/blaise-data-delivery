﻿###############################
# Data delivery pipeline script
###############################

. "$PSScriptRoot\..\functions\LoggingFunctions.ps1"
. "$PSScriptRoot\..\functions\FileFunctions.ps1"
. "$PSScriptRoot\..\functions\RestApiFunctions.ps1"

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

    # Create a sync group to capture statuses from each parallel process
    $origin = @{}
    $instruments | Foreach-Object { $origin.($_.name) = @{} }
    $sync = [System.Collections.Hashtable]::Synchronized($origin)

    # Deliver the instrument package with data for each active instrument
    $instruments | ForEach-Object -ThrottleLimit 3 -Parallel {
        $syncCopy = $using:sync
        $process = $syncCopy.$($PSItem.name)

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
            $deliveryFileName = GenerateDeliveryFilename -prefix "dd" -instrumentName $_.name

            if ($_.DeliverData -eq $false) {
                CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "inactive"
                continue
            }

            # Set data delivery status to started
            CreateDataDeliveryStatus -fileName $deliveryFileName -batchStamp $using:batchStamp -state "started"

            # Generate full file path for instrument
            $deliveryFile = "$env:TempPath\$deliveryFileName"

            # Download instrument package
            DownloadInstrumentPackage -serverParkName $_.serverParkName -instrumentName $_.name -fileName $deliveryFile

            # Create a temporary folder for processing instruments
            $processingFolder = CreateANewFolder -folderPath $env:TempPath -folderName "$($_.name)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"

            #Add manipula and instrument package to processing folder
            AddManipulaToProcessingFolder -processingFolder $processingFolder -deliveryFile $deliveryFile

            # Generate and add SPSS files
            AddSpssFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $_.name

            # Generate and add Ascii files
            AddAsciiFilesToDeliveryPackage -deliveryZip $deliveryFile -processingFolder $processingFolder -instrumentName $_.name

            # Upload instrument package to NIFI
            UploadFileToBucket -filePath $deliveryFile -bucketName $env:ENV_BLAISE_NIFI_BUCKET -deliveryFileName $deliveryFileName

            # Set data delivery status to generated
            UpdateDataDeliveryStatus -fileName $deliveryFileName -state "generated"
            $process.Status = "Completed"
        }
        catch {
            LogError("Error occured inside: $($_.Exception.Message) at: $($_.ScriptStackTrace)")
            Get-Error
            ErrorDataDeliveryStatus -fileName $deliveryFileName -state "errored" -error_info "An error has occured in delivering $deliveryFileName"
            $process.Status = "Errored"
        }
    }
}
catch {
    LogError("Error occured outside: $($_.Exception.Message) at: $($_.ScriptStackTrace)")
    Get-Error
    exit 1
}

# Check if any of the parallel processes returned an errored state and exit appropriately
$sync.Keys | ForEach-Object {
    if (![string]::IsNullOrEmpty($sync.$_.keys)) {
        # Create parameter hashtable to splat
        $param = $sync.$_

        if ($param.Status -eq "Errored") {
            exit 1
        }
    }
}
