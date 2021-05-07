. "$PSScriptRoot\LoggingFunctions.ps1"

function UploadFileToBucket {
    param (
        [string] $filePath,
        [string] $bucketName,
        [string] $deliveryFileName
    )

    If (-not (Test-Path $filePath)) {
        throw "$filePath not found"
    }

    If ([string]::IsNullOrEmpty($bucketName)) {
        throw "No bucket name provided" }

    If ([string]::IsNullOrEmpty($deliveryFileName)) {
        throw "No delivery zip has been provided" }

    LogInfo("Copying '$filePath' to '$bucketName'")
    try {
        & 'C:\Program Files (x86)\Google\Cloud SDK\google-cloud-sdk\bin\gsutil.cmd' rsync $filePath gs://$bucketName/$deliveryFileName
        LogInfo("Copied '$filePath' to '$bucketName'")
    }
    catch {
        LogError("Failed to upload '$filePath' to '$bucketName'")
    }
}
