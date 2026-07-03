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

    # GSUtils logs its progress bar to stderr, standard powershell core throws an error
    # when run from azure because of this, using cmd seemed to be the only option but
    # it swallows errors so we then have to check the output for exceptions
    $output = & cmd /c "gsutil 2>&1" cp $filePath gs://$bucketName/$deliveryFileName

    if ($output -Like "*exception*") {
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

    $output = & cmd /c "gsutil 2>&1" cp gs://$bucketName/$questionnaireFileName $filePath

    if ($output -Like "*exception*") {
        throw "Failed to download '$questionnaireFileName' from '$bucketName': '$output'"
    }

    LogInfo("Downloaded '$questionnaireFileName' from '$bucketName' to '$filePath'")
}
