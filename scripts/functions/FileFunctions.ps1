. "$PSScriptRoot\LoggingFunctions.ps1"
function GenerateDeliveryFilename {
    param (
        [string] $prefix,
        [string] $instrumentName,
        [datetime] $dateTime = (Get-Date),
        [string] $fileExt = $env:PackageExtension
    )
    
    If ([string]::IsNullOrEmpty($prefix)) {
        throw [System.IO.ArgumentException] "No prefix provided" }

    If ([string]::IsNullOrEmpty($instrumentName)) {
        throw [System.IO.ArgumentException] "No instrument name argument provided" }

    If ([string]::IsNullOrEmpty($fileExt)) {
        throw [System.IO.ArgumentException] "No file extension argument provided" }        

    return "$($prefix)_$($instrumentName)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss")).$fileExt"            
}

function GenerateBatchFileName{
    param (
        [datetime] $dateTime = (Get-Date),
        [string] $SurveyType = $env:SurveyType
    )
    If ([string]::IsNullOrEmpty($SurveyType)) {
        throw [System.IO.ArgumentException] "No Survey Type has been provided" }

    return "$($env:SurveyType)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss"))"
}

function ExtractZipFile {
    param (
        [string] $zipFilePath,
        [string] $destinationPath
    )
    # Extract the file contents into the path
    Expand-Archive $zipFilePath -DestinationPath $destinationPath
    LogInfo("Extracting zip file '$zipFilePath' to path '$destinationPath'")
}

function AddFilesToZip {
    param (
        [string[]] $files,
        [string] $zipFilePath
    )
    
    If ($files.count -eq 0) {
        throw [System.IO.ArgumentException] "No files provided" 
    }

    If (-not (Test-Path $zipFilePath)) {
        throw [System.IO.FileNotFoundException] "$zipFilePath not found"
    }

    Compress-Archive -Path $files -Update -DestinationPath $zipFilePath
    LogInfo("Added the file(s) '$files' to the zip file '$zipFilePath'")
}

function DeleteFile {
    param (
        [string] $filePath
    )

    If (-not (Test-Path $filePath)) {
        throw [System.IO.FileNotFoundException] "$filePath not found"
    }

    Remove-Item $filePath
}