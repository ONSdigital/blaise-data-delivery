. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
function AddJSONFileToDeliveryPackage {
    param(
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $tempPath
    )

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found" 
    }

    If (-not (Test-Path $deliveryZip)) {
        throw "$deliveryZip not found" 
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "No questionnaire name provided" 
    }

    # Copy Manipula xml files to the processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\json\GenerateJSON.msux" -Destination $processingFolder

    try {
        # Generate XML file
        & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateJSON.msux" -A:True -Q:True -K:Meta="$processingFolder/$questionnaireName.bmix" -I:$processingFolder/$questionnaireName.bdbx -O:$processingFolder/$questionnaireName.json
        LogInfo("Generated .Json File for $deliveryZip")
    }
    catch {
        LogWarning("Generating Json Failed: $($_.Exception.Message)")
    }
    try {
        if ([string]::IsNullOrEmpty($subFolder)) {
            AddFilesToZip -pathTo7zip $tempPath -files "$processingFolder\*.json*" -zipFilePath $deliveryZip
            LogInfo("Added .JSON File to '$deliveryZip'")
        }
        else {
            Copy-Item -Path "$processingFolder/$($questionnaireName).json" -Destination $subFolder/$questionnaireName.json
            Copy-Item -Path "$processingFolder/$($questionnaireName).json.bdix" -Destination $subFolder/$questionnaireName.json
            LogInfo("Copied .JSON Files to $subFolder")

            AddFolderToZip -pathTo7zip $tempPath -folder $subFolder -zipFilePath $deliveryZip
            LogInfo("Added '$subFolder' to '$deliveryZip'")
        }
    }
    catch {
        LogWarning("Unable to add .Json file to $deliveryZip")
    }
}
