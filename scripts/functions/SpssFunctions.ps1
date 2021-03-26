. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function AddSpssFilesToInstrumentPackage {
    param(
        [string] $manipulaPackage = "$env:TempPath\SpssPackage.zip",
        [string] $instrumentPackage,
        [string] $instrumentName
    )

    If (-not (Test-Path $manipulaPackage)) {
        throw [System.IO.FileNotFoundException] "$manipulaPackage file not found"
    }
    
    If (-not (Test-Path $instrumentPackage)) {
        throw [System.IO.FileNotFoundException] "$instrumentPackage file not found"
    }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name argument provided"
    }

    # Create temporary folder to extract the package
    $tempPath = "$env:TempPath\$($instrumentName)-$(Get-Date -format "yyyyMMddHHmmss")"

    # Extract the instrument package and manipula files into the temporary folder
    ExtractZipFile -zipFilePath $instrumentPackage -destinationPath $tempPath
    ExtractZipFile -zipFilePath $manipulaPackage -destinationPath $tempPath

    # Generate SPS file
    & cmd.exe /c $tempPath\Manipula.exe "$tempPath\GenerateStatisticalScript.msux" -K:meta="$instrumentName.bmix" -H:"" -L:"" -N:oScript="$instrumentName,iFNames=,iData=$instrumentName.bdix" -P:"SPSS;;;;;;$instrumentName.asc;;;2;;64;;Y" -Q:True | Out-Null
    LogInfo("Generated the .SPS file")

    # Generate .ASC file
    & cmd.exe /c $tempPath\Manipula.exe "$tempPath\ExportData_$instrumentName.msux" -A:True -Q:True | Out-Null
    LogInfo("Generated the .ASC file")

    # Add the SPS, ASC & FPS files to the instrument package
    AddFilesToZip -files "$tempPath\*.sps","$tempPath\*.asc","$tempPath\*.fps" -zipFilePath $instrumentPackage
}