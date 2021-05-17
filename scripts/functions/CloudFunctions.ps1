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
        throw "No bucket name provided"
    }

    If ([string]::IsNullOrEmpty($deliveryFileName)) {
        throw "No delivery file name has been provided"
    }

    LogInfo("Copying '$filePath' to '$bucketName'")
    # GSUtils logs its progress bar to stderr, standard powershell core throws an error
    # when run from azure because of this, using cmd seemed to be the only option but
    # it swallows errors so we then have to check the output for exceptions
    $output = & cmd /c "gsutil 2>&1" cp $filePath gs://$bucketName/$deliveryFileName
    if ($output -Like "*exception*") {
        throw "Failed to upload '$filePath' to '$bucketName': '$output'"
    }
    LogInfo("Copied '$filePath' to '$bucketName'")
}
