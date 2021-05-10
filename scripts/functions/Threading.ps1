function CreateInstrumentSync {
    param(
        [array] $instruments
    )

    $origin = @{}
    $instruments | Foreach-Object { $origin.($_.name) = @{} }
    $sync = [System.Collections.Hashtable]::Synchronized($origin)
    return $sync
}

function GetProcess {
    param(
        [hastable] $instrument,
        [hastable] $sync
    )

    $syncCopy = $sync
    return $syncCopy.$($instrument.name)
}

function CheckSyncStatus {
    param(
        [hashtable] $sync
    )

    $sync.Keys | ForEach-Object {
        if (![string]::IsNullOrEmpty($sync.$_.keys)) {
            # Create parameter hashtable to splat
            $param = $sync.$_

            if ($param.Status -eq "Errored") {
                exit 1
            }
        }
    }
}
