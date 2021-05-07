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
        gsutil -D cp C:\blaise\temp\datadelivery\opn\dd_OPN2101A_07052021_124259.zip gs://ons-blaise-v2-dev-sam8-nifi-staging
        LogInfo("Copied '$filePath' to '$bucketName'")
    }
    catch {
        LogError("Failed to upload '$filePath' to '$bucketName'")
    }
}
