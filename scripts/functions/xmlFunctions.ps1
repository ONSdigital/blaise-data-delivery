. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
function AddXMLFileForDeliveryPackage {
    param(
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $instrumentName,
        [string] $subFolder,
        [string] $tempPath
    )

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found" 
    }

    If (-not (Test-Path $deliveryZip)) {
        throw "$deliveryZip not found" 
    }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw "No instrument name provided" 
    }

    # Copy Manipula xml files to the processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\xml\GenerateXML.msux" -Destination $processingFolder

    try {
        # Generate XML file, Export function no longer works in Blaise 5
        & cmd.exe /c $processingFolder/MetaViewer.exe -F:$processingFolder/$instrumentName.bmix -Export
        LogInfo("Generated .XML File for $deliveryZip")
    }
    catch {
        LogWarning("Generating XML Failed: $($_.Exception.Message)")
    }
    try {
        if ([string]::IsNullOrEmpty($subFolder)) {
            # Add the SPS, ASC & FPS files to the instrument package
            AddFilesToZip -pathTo7zip $tempPath -files "$processingFolder\*.xml" -zipFilePath $deliveryZip
            LogInfo("Added .XML File to $deliveryZip")
        }
        else {
            Copy-Item -Path "$processingFolder/$($instrumentName)_meta.xml" -Destination $subFolder/$instrumentName.xml
            LogInfo("Copied .XML File to $subFolder")

            AddFolderToZip -pathTo7zip $tempPath -folder $subFolder -zipFilePath $deliveryZip
            LogInfo("Added '$subFolder' to '$deliveryZip'")
        }
    }
    catch {
        LogWarning("Unable to add .XML file to $deliveryZip")
    }
}
