. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
function AddAsciiFilesToDeliveryPackage {
    param (
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $instrumentName,
        [string] $subFolder
    )

    Copy-Item -Path "$PSScriptRoot\..\manipula\ascii\GenerateAscii.msux" -Destination $processingFolder
    # Generate .ASC file
    try {
        & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateAscii.msux" -A:True -Q:True -K:Meta="$processingFolder/$instrumentName.bmix" -I:$processingFolder/$instrumentName.bdbx -O:$processingFolder/$instrumentName.asc
        LogInfo("Generated the .ASC file")
    }
    catch {
        LogWarning("Generating ASCII Failed for $instrumentName : $($_.Exception.Message)")
    }

    if ([string]::IsNullOrEmpty($subFolder))
    {      
        # Add the SPS, ASC & FPS files to the instrument package
        AddFilesToZip -pathTo7zip $env:TempPath -files "$processingFolder\*.asc", "$processingFolder\*.fps" -zipFilePath $deliveryZip
        LogInfo("Added .ASC File to $deliveryZip")
    }
    else {
        Copy-Item -Path "$processingFolder\*.asc", "$processingFolder\*.fps" -Destination $subFolder

        AddFolderToZip -pathTo7zip $env:TempPath -folder $subFolder -zipFilePath $deliveryZip  
        LogInfo("Added '$subFolder' to '$deliveryZip'")
    }
}
