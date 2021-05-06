. "$PSScriptRoot\LoggingFunctions.ps1"

function UploadFileToBucket {
    param (
        [string] $filePath,
        [string] $bucketName
    )

    If (-not (Test-Path $filePath)) {
        throw "$filePath not found"
    }

    If ([string]::IsNullOrEmpty($bucketName)) {
        throw "No bucket name provided" }

    LogInfo("Copying '$filePath' to '$bucketName'")
    gsutil cp $filePath gs://$bucketName   
}
