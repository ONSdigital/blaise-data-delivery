$source = "DataDelivery"
Import-Module Microsoft.PowerShell.Management -UseWindowsPowerShell -WarningAction SilentlyContinue

function LogInfo {
    param (
        $message
    )

    CreateSourceIfNotExists($source)
    Write-Host "Information: $message"
    Write-EventLog -LogName "Application" -Source $source -EventId 3000 -EntryType Information -Message "$($source): $message"
}

function LogWarning {
    param (
        $message
    )
    
    CreateSourceIfNotExists($source)
    Write-Host "Warning: $message"
    Write-EventLog -LogName "Application" -Source $source -EventId 3000 -EntryType Warning -Message "$($source): $message"
}

function LogError {
    param (
        $message
    )

    CreateSourceIfNotExists($source)
    Write-Host "Error: $message"
    Write-EventLog -LogName "Application" -Source $source -EventId 3000 -EntryType Error -Message "$($source): $message"
}

function CreateSourceIfNotExists {
    param (
        $dataSource
    )

    if(-Not [System.Diagnostics.EventLog]::SourceExists($dataSource))
    {
        New-EventLog -LogName Application -Source $dataSource
    }
}
