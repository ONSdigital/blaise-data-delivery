. "$PSScriptRoot\LoggingFunctions.ps1"

function GetIDToken {
    return Invoke-RestMethod -UseBasicParsing "http://metadata/computeMetadata/v1/instance/service-accounts/default/identity?audience=$env:ENV_DDS_CLIENT&format=full" -Headers @{'Metadata-Flavor' = 'Google' }
}

function CreateDataDeliveryStatus {
    param(
        [string] $fileName,
        [string] $batchStamp,
        [string] $state
    )
    try {
        If ([string]::IsNullOrEmpty($fileName)) {
            throw "No file name name provided"
        }

        If ([string]::IsNullOrEmpty($batchStamp)) {
            throw "No batch stamp name provided"
        }

        If ([string]::IsNullOrEmpty($state)) {
            throw "No state name provided"
        }

        $DDSBaseUrl = "$env:ENV_DDS_URL/v1/state"
        $JsonObject = [ordered]@{
            state = "$($state)"
            batch = "$($batchStamp)"
        }

        $url = "$DDSBaseUrl/$fileName"
        $body = $JsonObject | ConvertTo-Json
        $idToken = GetIDToken
        Invoke-RestMethod -UseBasicParsing $url -ContentType "application/json" -Method POST -Body $body -Headers @{ 'Authorization' = "Bearer $idToken" }
    }
    catch {
        LogError("Creating Data Delivery Status failed: $($_.Exception.Message) at: $($_.ScriptStackTrace) $($_.ScriptStackTrace) StatusCode: $($_.Exception.Response.StatusCode.value__) StatusDescription: $($_.Exception.Response.StatusDescription)")
        Get-Error
    }
}

function UpdateDataDeliveryStatus {
    param(
        [string] $fileName,
        [string] $state
    )
    try {
        If ([string]::IsNullOrEmpty($fileName)) {
            throw "No file name name provided"
        }

        If ([string]::IsNullOrEmpty($state)) {
            throw "No state name provided"
        }

        $DDSBaseUrl = "$env:ENV_DDS_URL/v1/state"
        $JsonObject = [ordered]@{
            state = "$($state)"
        }
        $url = "$DDSBaseUrl/$fileName"
        $body = $JsonObject | ConvertTo-Json
        $idToken = GetIDToken
        Invoke-RestMethod -UseBasicParsing $url -ContentType "application/json" -Method PATCH -Body $body -Headers @{ 'Authorization' = "Bearer $idToken" }
    }
    catch {
        LogError("Creating Data Delivery Status failed: $($_.Exception.Message) at: $($_.ScriptStackTrace) StatusCode: $($_.Exception.Response.StatusCode.value__) StatusDescription: $($_.Exception.Response.StatusDescription)")
        Get-Error
    }
}

function ErrorDataDeliveryStatus {
    param(
        [string] $fileName,
        [string] $state,
        [string] $error_info
    )
    try {
        If ([string]::IsNullOrEmpty($fileName)) {
            throw "No file name name provided"
        }

        If ([string]::IsNullOrEmpty($state)) {
            throw "No state name provided"
        }

        $DDSBaseUrl = "$env:ENV_DDS_URL/v1/state"
        $JsonObject = [ordered]@{
            state      = "$($state)"
            error_info = "$($error_info)"
        }
        $url = "$DDSBaseUrl/$fileName"
        $body = $JsonObject | ConvertTo-Json
        $idToken = GetIDToken
        Invoke-RestMethod -UseBasicParsing $url -ContentType "application/json" -Method PATCH -Body $body -Headers @{ 'Authorization' = "Bearer $idToken" }
    }
    catch {
        LogError("Creating Data Delivery Status failed: $($_.Exception.Message) at: $($_.ScriptStackTrace) StatusCode: $($_.Exception.Response.StatusCode.value__) StatusDescription: $($_.Exception.Response.StatusDescription)")
        Get-Error
    }
}
