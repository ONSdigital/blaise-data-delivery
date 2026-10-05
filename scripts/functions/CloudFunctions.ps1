. "$PSScriptRoot\LoggingFunctions.ps1"

function UploadFileToBucket {
    param (
        [string] $filePath,
        [string] $bucketName,
        [string] $deliveryFileName
    )

    If ([string]::IsNullOrEmpty($filePath)) {
        throw "filePath not provided"
    }

    If (-not (Test-Path $filePath)) {
        throw "$filePath not found"
    }

    If ([string]::IsNullOrEmpty($bucketName)) {
        throw "bucketName not provided"
    }

    If ([string]::IsNullOrEmpty($deliveryFileName)) {
        throw "deliveryFileName not provided"
    }

    $destination = "gs://$bucketName/$deliveryFileName"
    LogInfo("Uploading '$filePath' to '$destination'")

    # Capture stderr for diagnostics; gcloud's exit code determines success.
    $output = & gcloud storage cp $filePath $destination 2>&1
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        throw "Failed to upload '$filePath' to '$destination' (gcloud exit code $exitCode): '$($output -join [Environment]::NewLine)'"
    }

    LogInfo("Uploaded '$filePath' to '$bucketName'")
}

function DownloadFileFromBucket {
    param (
    [string] $questionnaireFileName,
    [string] $bucketName,
    [string] $filePath
    )

    If ([string]::IsNullOrEmpty($questionnaireFileName)) {
        throw "questionnaireFileName not provided"
    }

    If ([string]::IsNullOrEmpty($bucketName)) {
        throw "bucketName not provided"
    }

    If ([string]::IsNullOrEmpty($filePath)) {
        throw "filePath not provided"
    }

    $source = "gs://$bucketName/$questionnaireFileName"
    LogInfo("Downloading '$source' to '$filePath'")

    $output = & gcloud storage cp $source $filePath 2>&1
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        throw "Failed to download '$source' to '$filePath' (gcloud exit code $exitCode): '$($output -join [Environment]::NewLine)'"
    }

    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "gcloud storage cp reported success for '$source' but did not create destination file '$filePath'. Output: '$($output -join [Environment]::NewLine)'"
    }

    LogInfo("Downloaded '$source' to '$filePath'")
}
