. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
. "$PSScriptRoot\CloudFunctions.ps1"

function AddSpssFilesToDeliveryPackage {
    param(
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $instrumentName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $tempPath
    )

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder file not found"
    }

    If (-not (Test-Path $deliveryZip)) {
        throw "$deliveryZip file not found"
    }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw "No instrument name argument provided"
    }

    if (-not (Test-Path "$processingFolder\spss")) {
        # Copy Manipula spss files to the processing folder
        Copy-Item -Path "$PSScriptRoot\..\manipula\spss\*" -Destination $processingFolder

        # Generate SPS file
        try {
            & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateStatisticalScript.msux" -K:meta="$instrumentName.bmix" -H:"" -L:"" -N:oScript="$instrumentName,iFNames=,iData=$instrumentName.bdix" -P:"SPSS;;;;;;$instrumentName.asc;;;2;;64;;Y" -Q:True
            LogInfo("Generated the .SPSS file")
            #create an sps folder
            CreateANewFolder -folderPath $processingFolder -folderName "spss"
            #copying the files needed to create an Ascii file
            Copy-Item -Path "$processingFolder\*.sps" -Destination "$processingFolder/spss"
            #adding the above files to the delivery zip
            AddFolderToZip -pathTo7zip $tempPath -folder "$processingFolder/spss" -zipFilePath $deliveryZip
            #uploading the delivery file back to the dqs bucket with the
            UploadFileToBucket -filePath $deliveryZip -bucketName $dqsBucket -deliveryFileName "$($instrumentName).bpkg"
        }
        catch {
            LogWarning("Generating SPS and FPS Failed for $instrumentName : $($_.Exception.Message)")
            Get-Error
        }
    }
    else {
        Copy-Item -Path "$processingFolder/spss/*" -Destination $processingFolder -verbose
    }

    if ([string]::IsNullOrEmpty($subFolder)) {
        # Add the SPS, ASC & FPS files to the instrument package
        AddFilesToZip -pathTo7zip $tempPath -files "$processingFolder\*.sps" -zipFilePath $deliveryZip
        LogInfo("Added .SPS Files to $deliveryZip")
    }
    else {
        Copy-Item -Path "$processingFolder\*.sps" -Destination $subFolder

        AddFolderToZip -pathTo7zip $tempPath -folder $subFolder -zipFilePath $deliveryZip
        LogInfo("Added '$subFolder' to '$deliveryZip'")
    }
}
