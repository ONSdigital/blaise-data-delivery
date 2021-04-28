. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
function AddXMLFileForDeliveryPackage{
    param(
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $instrumentName
    )

    If (-not (Test-Path $processingFolder)) {
        throw [System.IO.FileNotFoundException] "$processingFolder not found" }

    If (-not (Test-Path $deliveryZip)) {
        throw [System.IO.FileNotFoundException] "$deliveryZip not found" }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name provided" }

    # Copy Manipula xml files to the processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\xml\GenerateXML.msux" -Destination $processingFolder

    #Gets the folder name of the processing folder
    $deliveryFolderName = Split-Path $processingFolder -Leaf
    
    # Create a folder within the temporary folder for generating XML
    $deliveryFolder = CreateANewFolder -folderPath $processingFolder -folderName $deliveryFolderName

    try {
        # Generate XML file, Export function no longer works in Blaise 5 
        & cmd.exe /c $processingFolder/MetaViewer.exe -F:$processingFolder/$instrumentName.bmix -Export
        Copy-Item -Path "$processingFolder/$($instrumentName)_meta.xml" -Destination $deliveryFolder/$instrumentName.xml
        LogInfo("Generated .XML File for $deliveryZip")
    }
    catch {
        LogWarning("Generating XML Failed: $($_.Exception.Message)")
    }
    try {
        AddFolderToZip -folder $deliveryFolder -zipFilePath $deliveryZip
    }
    catch {
        LogWarning("Unable to add .XML file to $deliveryZip")
    }
}
