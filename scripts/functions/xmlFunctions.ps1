. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
function GenerateXMLFileForDeliveryPackage{
    param(
        [string] $processingFolder,
        [string] $deliveryFolder,
        [string] $deliveryZip,
        [string] $instrumentName
    )

    If ([string]::IsNullOrEmpty($processingFolder)) {
        throw [System.IO.ArgumentException] "No temporary folder provided" }
        
    If ([string]::IsNullOrEmpty($deliveryFolder)) {
        throw [System.IO.ArgumentException] "No delivery folder provided" }

    If ([string]::IsNullOrEmpty($deliveryZip)) {
        throw [System.IO.ArgumentException] "No delivery file provided" }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name provided" }

# Generate XML
try {
    & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateXML.msux" -A:True -Q:True -K:OPXMeta="$processingFolder/$instrumentName.bmix" -I:$processingFolder/$instrumentName.bdbx -O:$deliveryFolder/$instrumentName.xml
}
catch {
    LogWarning("Generating XML Failed: $_.Exception.Message")
}
    AddFolderToZip -files $deliveryFolder, -zipFilePath $deliveryZip
}
