function GenerateDeliveryFileName {
    param (
        [string] $prefix,
        [string] $suffix,
        [string] $questionnaireName,
        [datetime] $dateTime = (Get-Date),
        [string] $fileExt
    )

    If ([string]::IsNullOrEmpty($prefix)) {
        throw "prefix not provided"
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "questionnaireName not provided"
    }

    If ([string]::IsNullOrEmpty($fileExt)) {
        throw "fileExt not provided"
    }

    return "$($prefix)_$($questionnaireName)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss"))$($suffix).$fileExt"
}

function GenerateBatchFileName {
    param (
        [datetime] $dateTime = (Get-Date),
        [string] $surveyType
    )

    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "surveyType not provided"
    }

    return "$($surveyType)_$($dateTime.ToString("ddMMyyyy"))_$($dateTime.ToString("HHmmss"))"
}

function ExtractZipFile {
    param (
        [string] $pathTo7zip,
        [string] $zipFilePath,
        [string] $destinationPath
    )

    If ([string]::IsNullOrEmpty($pathTo7zip)) {
        throw "pathTo7zip not provided"
    }

    If (-not (Test-Path $pathTo7zip)) {
        throw "$pathTo7zip not found"
    }

    If ([string]::IsNullOrEmpty($zipFilePath)) {
        throw "zipFilePath not provided"
    }

    If (-not (Test-Path $zipFilePath)) {
        throw "$zipFilePath not found"
    }

    & $pathTo7zip\7za x $zipFilePath -o"$destinationPath" > $null # x = extract and keep folder structure
}

function AddFilesToZip {
    param (
        [string] $pathTo7zip,
        [string[]] $files,
        [string] $zipFilePath
    )

    If ([string]::IsNullOrEmpty($pathTo7zip)) {
        throw "pathTo7zip not provided"
    }

    If (-not (Test-Path $pathTo7zip)) {
        throw "$pathTo7zip not found"
    }

    If ($files.count -eq 0) {
        throw "files not provided"
    }

    If ([string]::IsNullOrEmpty($zipFilePath)) {
        throw "zipFilePath not provided"
    }

    & $pathTo7zip\7za a $zipFilePath $files > $null # a = add
}

function AddFolderToZip {
    param (
        [string] $pathTo7zip,
        [string] $folder,
        [string] $zipFilePath
    )

    If (-not (Test-Path $folder)) {
        throw "$folder not found"
    }

    & $pathTo7zip\7za a $zipFilePath $folder > $null # a = add
}

function CreateFolder {
    param (
        [string] $folderPath,
        [string] $folderName
    )

    If ([string]::IsNullOrEmpty($folderPath)) {
        throw "folderPath not provided"
    }

    If ([string]::IsNullOrEmpty($folderName)) {
        throw "folderName not provided"
    }

    if (-not (Test-Path $folderPath\$folderName)) {
        New-Item -Path $folderPath -Name $folderName -ItemType "directory" | Out-Null
    }

    return "$folderPath\$folderName"
}

function ConvertJsonFileToObject {
    param (
        [string] $jsonFile
    )
    return Get-Content -Path $jsonFile | ConvertFrom-Json
}
