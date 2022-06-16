function CreateQuestionnaireSync {
    param(
        [array] $questionnaires
    )

    $origin = @{}
    $questionnaires | Foreach-Object { $origin.($_.name) = @{} }
    $sync = [System.Collections.Hashtable]::Synchronized($origin)
    return $sync
}

function GetProcess {
    param(
        $questionnaire,
        [System.Collections.Hashtable] $sync
    )

    $syncCopy = $sync
    return $syncCopy.$($questionnaire.name)
}

function CheckSyncStatus {
    param(
        [System.Collections.Hashtable] $sync
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
