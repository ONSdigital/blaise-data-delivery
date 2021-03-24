. "$PSScriptRoot\LoggingFunctions.ps1"

function UploadFileToBucket {
    param (
        [string] $fileName,
        [string] $bucketName
    )

    If ([string]::IsNullOrEmpty($fileName)) {
        throw [System.IO.ArgumentException] "No file name provided" }

    If ([string]::IsNullOrEmpty($bucketName)) {
        throw [System.IO.ArgumentException] "No bucket name provided" }

    gsutil cp $fileName gs://$bucketName
    LogInfo("Pushed instrument '$fileName' to the NIFI bucket '$bucketName'")        
}