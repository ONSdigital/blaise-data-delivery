. "$PSScriptRoot\FileFunctions.ps1"
function AddManipulaToProcessingFolder {
    param (
        [string] $manipulaPackage,
        [string] $processingFolder,
        [string] $tempPath
    )

    If (-not (Test-Path $manipulaPackage)) {
        throw "$manipulaPackage not found"
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found"
    }

    # Extract Manipula files to the processing folder
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $manipulaPackage -destinationPath $processingFolder
}
