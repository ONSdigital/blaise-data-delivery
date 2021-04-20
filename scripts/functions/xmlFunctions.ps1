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
        # Generate XML file
        & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateXML.msux" -A:True -Q:True -K:OPXMeta="$processingFolder/$instrumentName.bmix" -I:$processingFolder/$instrumentName.bdbx -O:$deliveryFolder/$instrumentName.xml
        LogInfo("Generated .XML File for $deliveryZip")
    }
    catch {
        LogWarning("Generating XML Failed: $_.Exception.Message")
    }
    try {
        AddFolderToZip -folder $deliveryFolder -zipFilePath $deliveryZip
    }
    catch {
        LogWarning("Unable to add .XML file to $deliveryZip")
    }
}
