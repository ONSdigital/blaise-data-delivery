. "$PSScriptRoot\FileFunctions.ps1"
function AddManipulaToProcessingFolder {
    param (
        [string] $manipulaPackage,
        [string] $processingFolder,
        [string] $deliveryFile,
        [string] $tempPath
    )

    If (-not (Test-Path $manipulaPackage)) {
        throw "$manipulaPackage not found"
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found"
    }

    If (-not (Test-Path $deliveryFile)) {
        throw "$deliveryFile not found"
    }

    # Extract Manipula files to the processing folder
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $manipulaPackage -destinationPath $processingFolder

    # Extact Instrument Package
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $deliveryFile -destinationPath $processingFolder
}
