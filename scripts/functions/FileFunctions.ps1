. "$PSScriptRoot\LoggingFunctions.ps1"
function GenerateDeliveryFilename {
    param (
        [string] $prefix,
        [string] $suffix,
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

    return "$($prefix)_$($questionnaireName)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss"))$($suffix).$fileExt"
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

function DeleteFilesInZip {
    param (
        [string] $pathTo7zip,
        [string] $zipFilePath,
        [string] $fileName
    )

    If (-not (Test-Path $zipFilePath)) {
        throw "$zipFilePath not found"
    }
    #7 zip CLI - a = add / append - Zip file to Create / append too - Files to add to the zip
    & $pathTo7zip\7za d $zipFilePath $fileName -r
    LogInfo("Deleted the file(s) $fileName in the zip file '$zipFilePath'")
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

function RenameQuestionnaireFiles {
    param (
        [string] $tempPath,
        [string] $processingFolder,
        [string] $deliveryFile,
        [string] $questionnaireNameFrom,
        [string] $questionnaireNameTo
    )

    If (-not (Test-Path $tempPath)) {
        throw "$tempPath not found" 
    }

    If (-not (Test-Path $deliveryFile)) {
        throw "$deliveryFile not found" 
    }

    If ([string]::IsNullOrEmpty($questionnaireNameFrom)) {
        throw "No questionnaireNameFrom provided" 
    }

    If ([string]::IsNullOrEmpty($questionnaireNameTo)) {
        throw "No questionnaireNameTo provided" 
    }    

    try {
        $extractPath = "$($processingFolder)\{$(New-Guid)}"
        ExtractZipFile -pathTo7zip $tempPath -zipFilePath $deliveryFile -destinationPath $extractPath
        
        Rename-Item -Path "$extractPath\$questionnaireNameFrom.bmix" -NewName "$extractPath\$questionnaireNameTo.bmix"
        Rename-Item -Path "$extractPath\$questionnaireNameFrom.bdix" -NewName "$extractPath\$questionnaireNameTo.bdix"
        Rename-Item -Path "$extractPath\$questionnaireNameFrom.bdbx" -NewName "$extractPath\$questionnaireNameTo.bdbx"

        AddFilesToZip -pathTo7zip $tempPath -files "$extractPath\$questionnaireNameTo.bmix","$extractPath\$questionnaireNameTo.bdix","$extractPath\$questionnaireNameTo.bdbx" -zipFilePath $deliveryFile
        DeleteFilesInZip -pathTo7zip $tempPath -zipFilePath $deliveryFile -fileName "$questionnaireNameFrom.*"
    }
    catch {
        LogError("Renaming unedited questionnaire files Failed for $questionnaireName : $($_.Exception.Message)")
    }   
}
