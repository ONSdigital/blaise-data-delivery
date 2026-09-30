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

    LogInfo("Uploading '$filePath' to '$bucketName'")

    # Capture stderr with stdout because gcloud storage may write progress and errors there.
    # Use the exit code as the primary failure check, with output text as a fallback.
    
    $output = & gcloud storage cp $filePath gs://$bucketName/$deliveryFileName 2>&1

    if ($LASTEXITCODE -ne 0 -or $output -Like "*exception*") {
        throw "Failed to upload '$filePath' to '$bucketName': '$output'"
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

    LogInfo("Downloading '$questionnaireFileName' from '$bucketName' to '$filePath'") 

    $output = & gcloud storage cp gs://$bucketName/$questionnaireFileName $filePath 2>&1
    
    if ($LASTEXITCODE -ne 0 -or $output -Like "*exception*") {
        throw "Failed to download '$questionnaireFileName' from '$bucketName': '$output'"
    }

    LogInfo("Downloaded '$questionnaireFileName' from '$bucketName' to '$filePath'")
}
