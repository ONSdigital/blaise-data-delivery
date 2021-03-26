. "$PSScriptRoot\LoggingFunctions.ps1"

function CreateDataDeliveryStatus{
        param(
            [string] $fileName,
            [string] $batchStamp,
            [string] $state
        )
    try
    {
        If ([string]::IsNullOrEmpty($fileName)) {
            throw [System.IO.ArgumentException] "No file name name provided" }
    
        If ([string]::IsNullOrEmpty($batchStamp)) {
            throw [System.IO.ArgumentException] "No batch stamp name provided" }
    
        If ([string]::IsNullOrEmpty($state)) {
            throw [System.IO.ArgumentException] "No state name provided" }

        $DDSBaseUrl = "$env:ENV_DDS_URL/v1/state"
        $JsonObject = [ordered]@{
                    state = "$($state)"
                    batch = "$($batchStamp)"
                }

        $url = "$DDSBaseUrl/$fileName"
        $body = $JsonObject | ConvertTo-Json
        Invoke-RestMethod -UseBasicParsing $url -ContentType "application/json" -Method POST -Body $body
    }
    catch
    {
        LogError("Creating Data Delivery Status failed: $($_.ScriptStackTrace)")
    }
}

function UpdateDataDeliveryStatus{
        param(
            [string] $fileName,
            [string] $state
        )
    try
    {
        If ([string]::IsNullOrEmpty($fileName)) {
            throw [System.IO.ArgumentException] "No file name name provided" }
    
        If ([string]::IsNullOrEmpty($state)) {
            throw [System.IO.ArgumentException] "No state name provided" }

        $DDSBaseUrl = "$env:ENV_DDS_URL/v1/state"
        $JsonObject = [ordered]@{
                    state = "$($state)"
                }
        $url = "$DDSBaseUrl/$fileName"
        $body = $JsonObject | ConvertTo-Json
        Invoke-RestMethod -UseBasicParsing $url -ContentType "application/json" -Method PATCH -Body $body
    }
    catch
    {
        LogError("Updated Data Delivery Status failed: $($_.ScriptStackTrace)")
    }
}

function ErrorDataDeliveryStatus{
        param(
            [string] $fileName,
            [string] $state,
            [string] $error_info
        )
    try
    {
        If ([string]::IsNullOrEmpty($fileName)) {
            throw [System.IO.ArgumentException] "No file name name provided" }
    
        If ([string]::IsNullOrEmpty($state)) {
            throw [System.IO.ArgumentException] "No state name provided" }

        If ([string]::IsNullOrEmpty($batchStamp)) {
            throw [System.IO.ArgumentException] "No batch stamp name provided" }

        $DDSBaseUrl = "$env:ENV_DDS_URL/v1/state"
        $JsonObject = [ordered]@{
                    state = "$($state)"
                    error_info = "$($error_info)"
                }
        $url = "$DDSBaseUrl/$fileName"
        $body = $JsonObject | ConvertTo-Json
        Invoke-RestMethod -UseBasicParsing $url -ContentType "application/json" -Method PATCH -Body $body
    }
    catch
    {
        LogError("Sending Error message to Data Delivery Status failed: $($_.ScriptStackTrace)")
    }
}
