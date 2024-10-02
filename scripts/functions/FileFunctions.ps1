. "$PSScriptRoot\LoggingFunctions.ps1"
function GenerateDeliveryFilename {
    param (
        [string] $prefix,
        [string] $questionnaireName,
        [datetime] $dateTime = (Get-Date),
        [string] $fileExt
    )

    If ([string]::IsNullOrEmpty($prefix)) {
        throw "No prefix provided"
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "No questionnaire name argument provided"
    }

    If ([string]::IsNullOrEmpty($fileExt)) {
        throw "No file extension argument provided"
    }

    return "$($prefix)_$($questionnaireName)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss")).$fileExt"
}

function GenerateBatchFileName {
    param (
        [datetime] $dateTime = (Get-Date),
        [string] $surveyType
    )
    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "No Survey Type has been provided"
    }

    return "$($surveyType)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss"))"
}

function ExtractZipFile {
    param (
        [string] $pathTo7zip,
        [string] $zipFilePath,
        [string] $destinationPath
    )

    If (-not (Test-Path $zipFilePath)) {
        throw "$zipFilePath not found"
    }

    # 7zip extract - x = extract and keep folder structure of zup - o = output file can't have a space between -o and folder
    & $pathTo7zip\7za x $zipFilePath -o"$destinationPath"

    LogInfo("Extracting zip file '$zipFilePath' to path '$destinationPath'")
}

function AddFilesToZip {
    param (
        [string] $pathTo7zip,
        [string[]] $files,
        [string] $zipFilePath
    )

    If ($files.count -eq 0) {
        throw "No files provided"
    }

    If (-not (Test-Path $zipFilePath)) {
        throw "$zipFilePath not found"
    }
    #7 zip CLI - a = add / append - Zip file to Create / append too - Files to add to the zip
    & $pathTo7zip\7za a $zipFilePath $files
    LogInfo("Added the file(s) '$files' to the zip file '$zipFilePath'")
}

function AddFolderToZip {
    param (
        [string] $pathTo7zip,
        [string] $folder,
        [string] $zipFilePath
    )

    If (-not (Test-Path $folder)) {
        throw "$zipFilePath not found"
    }

    If (-not (Test-Path $zipFilePath)) {
        throw "$zipFilePath not found"
    }

    #7 zip CLI - a = add / append - Zip file to Create / append too - Files to add to the zip
    & $pathTo7zip\7za a $zipFilePath $folder
    LogInfo("Added the folder '$folder' to the zip file '$zipFilePath'")
}

function CreateANewFolder {
    param (
        [string] $folderPath,
        [string] $folderName
    )
    If ([string]::IsNullOrEmpty($folderPath)) {
        throw "No Path to the new folder provided"
    }
    If ([string]::IsNullOrEmpty($folderName)) {
        throw "No folder name provided"
    }

    if (-not (Test-Path $folderPath\$folderName)) {
        Write-Host "creating new folder: $folderName in $folderPath"
        New-Item -Path $folderPath -Name $folderName -ItemType "directory" | Out-Null
    }

    return "$folderPath\$folderName"
}

function GetFolderNameFromAPath {
    param (
        [string] $folderPath
    )
    If ([string]::IsNullOrEmpty($folderPath)) {
        throw "No Path to the new folder provided"
    }
    return Split-Path $processingFolder -Leaf
}

function ConvertJsonFileToObject {
    param (
        [string] $jsonFile
    )  

    return Get-Content -Path $jsonFile | ConvertFrom-Json
}

function CreateUneditedQuestionnaireFiles {
    param (
        [string] $tempPath,
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $questionnaireName
    )

    If (-not (Test-Path $tempPath)) {
        throw "$tempPath not found" 
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found" 
    }

    If (-not (Test-Path $deliveryZip)) {
        throw "$deliveryZip not found" 
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "No questionnaire name provided" 
    }

    try {
        ExtractZipFile -pathTo7zip $tempPath -zipFilePath $deliveryZip -destinationPath $processingFolder
        LogInfo("Extracted the delivery zip")

        Rename-Item -Path "$processingFolder\$questionnaireName.bmix" -NewName "$($processingFolder)\$($questionnaireName)_UNEDITED.bmix"
        LogInfo("Renamed bmix file")

        AddFilesToZip -pathTo7zip $tempPath -files "$($processingFolder)\$($questionnaireName)_UNEDITED.bmix" -zipFilePath $deliveryZip
        LogInfo("Added bmix file to the delivery zip")

    }
    catch {
        LogWarning("Creating unedited questionnaire files Failed for $questionnaireName : $($_.Exception.Message)")
    }

   
}