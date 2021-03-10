###############################
# SPSS file script
###############################

param([string]$instrumentName)

If ([string]::IsNullOrEmpty($instrumentName)) {
    throw [System.IO.ArgumentException] "No instrument name argument provided"
}

If (-not (Test-Path "$instrumentName.$env:PackageExtension")) {
    throw [System.IO.FileNotFoundException] "$instrumentName.$env:PackageExtension file not found"
}

# Cannot extract from a zip archive without a .ZIP extension
$instrumentPackage = "$instrumentName.$env:PackageExtension"

# Create temporary folder to extract the package
$tempPath = [guid]::NewGuid()

# Extract the instrument package into the temporary folder
Expand-Archive $instrumentPackage -DestinationPath $tempPath

# Download manipula package from CGP bucket
$manipulaPackage = "SpssPackage.zip"
gsutil cp gs://$env:ENV_BLAISE_NIFI_BUCKET/$manipulaPackage $manipulaPackage  

# Extract the manipula package into the temporary folder
Expand-Archive $manipulaPackage -DestinationPath $tempPath

# Generate SPS file
& .\$tempPath\Manipula.exe "$tempPath\GenerateStatisticalScript.msux" -K:meta="$instrumentName.bmix" -H:"" -L:"" -N:oScript="$instrumentName,iFNames=,iData=$instrumentName.bdix" -P:"SPSS;;;;;;$instrumentName.asc;;;2;;64;;Y" | Out-Null

# Generate .ASC file
& .\$tempPath\Manipula.exe "$tempPath\ExportData_$instrumentName.msux" -A:True | Out-Null

# Add the SPS, ASC & FPS files to the instrument package
Compress-Archive -Path $tempPath\*.sps,$tempPath\*.asc,$tempPath\*.fps -Update -DestinationPath $instrumentPackage

# Remove the manipula package
Remove-Item $manipulaPackage

# Remove the temporary files
Remove-Item $tempPath -Recurse


