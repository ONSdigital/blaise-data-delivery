. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
function AddJsonFileForDeliveryPackage{
    param(
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $instrumentName,
        [string] $subFolder
    )

    If (-not (Test-Path $processingFolder)) {
        throw [System.IO.FileNotFoundException] "$processingFolder not found" }

    If (-not (Test-Path $deliveryZip)) {
        throw [System.IO.FileNotFoundException] "$deliveryZip not found" }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name provided" }

    # Copy Manipula xml files to the processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\json\GenerateJSON.msux" -Destination $processingFolder

    try {
        # Generate XML file
        & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateJSON.msux" -A:True -Q:True -K:Meta="$processingFolder/$instrumentName.bmix" -I:$processingFolder/$instrumentName.bdbx -O:$subFolder/$instrumentName.json
        LogInfo("Generated .Json File for $deliveryZip")
    }
    catch {
        LogWarning("Generating Json Failed: $($_.Exception.Message)")
    }
    try {
        if ([string]::IsNullOrEmpty($subFolder))
        {
            AddFilesToZip -pathTo7zip $env:TempPath -files "$processingFolder\*.json" -zipFilePath $deliveryZip
            LogInfo("Added .JSON File to '$deliveryZip'")
        }
        else {
            AddFolderToZip -pathTo7zip $env:TempPath -folder $subFolder -zipFilePath $deliveryZip
            LogInfo("Added '$subFolder' to '$deliveryZip'")
        }
    }
    catch {
        LogWarning("Unable to add .Json file to $deliveryZip")
    }
}
