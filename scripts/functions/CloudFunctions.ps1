. "$PSScriptRoot\LoggingFunctions.ps1"

function UploadFileToBucket {
    param (
        [string] $filePath,
        [string] $bucketName
    )

    If ([string]::IsNullOrEmpty($filePath)) {
        throw [System.IO.ArgumentException] "No file name provided" }

    If ([string]::IsNullOrEmpty($bucketName)) {
        throw [System.IO.ArgumentException] "No bucket name provided" }

    gsutil cp $filePath gs://$bucketName
    LogInfo("Pushed instrument '$filePath' to the NIFI bucket '$bucketName'")        
}