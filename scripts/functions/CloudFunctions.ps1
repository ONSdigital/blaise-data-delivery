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
    $output = & cmd /c "gsutil 2>&1" cp $filePath gs://foo-$bucketName/$deliveryFileName
    # $output = gsutil cp $filePath gs://$bucketName/$deliveryFileName 2>&1 | %{ "$_" }
    if ($output -Like "*exception*") {
        throw "Failed to upload '$filePath' to '$bucketName': '$output'"
    }
    LogInfo("Copied '$filePath' to '$bucketName'")
}
