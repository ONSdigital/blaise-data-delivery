. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function AddSpssMetadataToDelivery {
    param(
        [string] $processingFolder,
        [string] $questionnaireName,
        [string] $subFolder
    )

    If ([string]::IsNullOrEmpty($processingFolder)) {
        throw "processingFolder not provided"
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found"
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "questionnaireName not provided"
    }

    # Copy Manipula statistical metadata scripts to processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\StatisticalMetadata\*" -Destination $processingFolder -Force

    # Generate SPSS metadata
    try {
        $manipulaPath = Join-Path $processingFolder "Manipula.exe"
        $msuxPath = Join-Path $processingFolder "GenerateStatisticalScript.msux"
        $bmixPath = Join-Path $processingFolder "$questionnaireName.bmix"
        $arguments = @(
            "`"$msuxPath`"",
            "-K:meta=`"$bmixPath`"",
            "-P:SPSS;;;;;;;;;2;;;;Y;;;;;;N",
            "-Q:True"
        )
        $process = Start-Process -FilePath $manipulaPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -eq 0) {
            LogInfo("Successfully generated SPSS metadata for $questionnaireName")
        }
        else {
            LogWarning("Failed generating SPSS metadata for $questionnaireName")
            LogWarning("Manipula exit code - $process.ExitCode")
        }
    }
    catch {
        LogWarning("Failed generating SPSS metadata for $questionnaireName")
        LogWarning("$($_.Exception.Message)")
        return
    }

    # Move output to subfolder if specified
    if (-not [string]::IsNullOrEmpty($subFolder)) {
        LogInfo("Moving SPSS metadata output to subfolder")
        Move-Item -Path "$processingFolder\*.sps" -Destination $subFolder -Force -ErrorAction SilentlyContinue
        LogInfo("Moved SPSS metadata output to subfolder")
    }
    else {
        LogInfo("SPSS metadata output not moved to subfolder")
    }
}
