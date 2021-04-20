. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function AddSpssFilesToDeliveryPackage {
    param(
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $instrumentName
    )

    If (-not (Test-Path $processingFolder)) {
        throw [System.IO.FileNotFoundException] "$processingFolder file not found"
    }
    
    If (-not (Test-Path $deliveryZip)) {
        throw [System.IO.FileNotFoundException] "$deliveryZip file not found"
    }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name argument provided"
    }

    # Copy Manipula spss files to the processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\spss\*" -Destination $processingFolder

    # Generate SPS file
    try {
        & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\GenerateStatisticalScript.msux" -K:meta="$instrumentName.bmix" -H:"" -L:"" -N:oScript="$instrumentName,iFNames=,iData=$instrumentName.bdix" -P:"SPSS;;;;;;$instrumentName.asc;;;2;;64;;Y" -Q:True
        LogInfo("Generated the .SPS file")
    }
    catch {
        LogWarning("Generating SPS and FPS Failed for $instrumentName : $_.Exception.Message")
    }

    # Generate .ASC file
    try {
        & cmd.exe /c $processingFolder\Manipula.exe "$processingFolder\ExportData_$instrumentName.msux" -A:True -Q:True
        LogInfo("Generated the .ASC file")
    }
    catch {
        LogWarning("Generating ASCII Failed for $instrumentName : $_.Exception.Message")
    }

    # Add the SPS, ASC & FPS files to the instrument package
    AddFilesToZip -files "$processingFolder\*.sps","$processingFolder\*.asc","$processingFolder\*.fps" -zipFilePath $deliveryZip
}
