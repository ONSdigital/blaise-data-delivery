. "$PSScriptRoot\FileFunctions.ps1"
function AddManipulaToProcessingFolder {
    param (
        [string] $manipulaPackage = "$env:TempPath/manipula.zip",
        [string] $processingFolder,
        [string] $deliveryFile
    )

    If (-not (Test-Path $manipulaPackage)) {
        throw [System.IO.FileNotFoundException] "$manipulaPackage not found" }

    If (-not (Test-Path $processingFolder)) {
        throw [System.IO.FileNotFoundException] "$processingFolder not found" }

    If (-not (Test-Path $deliveryFile)) {
        throw [System.IO.FileNotFoundException] "$deliveryFile not found" }
        
    # Extract Manipula files to the processing folder
    ExtractZipFile -zipFilePath "$env:TempPath\Manipula.zip" -destinationPath $processingFolder

    # Extact Instrument Package 
    ExtractZipFile -zipFilePath $deliveryFile -destinationPath $processingFolder
}
