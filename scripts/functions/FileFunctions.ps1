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
    
    If (-not (Test-Path $zipFilePath)) {
        throw [System.IO.FileNotFoundException] "$zipFilePath not found"
    }
    
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

function AddFolderToZip {
    param (
        [string] $folder,
        [string] $zipFilePath
    )
    
    If (-not (Test-Path $folder)) {
        throw [System.IO.FileNotFoundException] "$zipFilePath not found"
    }

    If (-not (Test-Path $zipFilePath)) {
        throw [System.IO.FileNotFoundException] "$zipFilePath not found"
    }

    Compress-Archive -Path $folder -Update -DestinationPath $zipFilePath
    LogInfo("Added the folder '$folder' to the zip file '$zipFilePath'")
}

function CreateANewFolder {
    param (
        [string] $folderPath,
        [string] $folderName
    )
    If ([string]::IsNullOrEmpty($folderPath)) {
        throw [System.IO.ArgumentException] "No Path to the new folder provided" 
    }
    If ([string]::IsNullOrEmpty($folderName)) {
        throw [System.IO.ArgumentException] "No folder name provided" 
    }

    if (-not (Test-Path $folderPath\$folderName))
    {
        Write-Host "creating new folder: $folderName in $folderPath"
        New-Item -Path $folderPath -Name $folderName -ItemType "directory" | Out-Null
    }
    
    return "$folderPath\$folderName"
}
