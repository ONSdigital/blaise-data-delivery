. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function AddJsonDataToDelivery {
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

    # Copy Manipula JSON data scripts to processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\JsonData\*" -Destination $processingFolder -Force

    # Generate JSON data
    try {
        $manipulaPath = Join-Path $processingFolder "Manipula.exe"
        $msuxPath = Join-Path $processingFolder "GenerateJsonData.msux"
        $bmixPath = Join-Path $processingFolder "$questionnaireName.bmix"
        $bdbxPath = Join-Path $processingFolder "$questionnaireName.bdbx"
        $outputPath = Join-Path $processingFolder "$questionnaireName.json"
        $arguments = @(
            "`"$msuxPath`"",
            "-K:Meta=`"$bmixPath`"",
            "-I:`"$bdbxPath`"",
            "-O:`"$outputPath`"",
            "-Q:True"
        )
        $process = Start-Process -FilePath $manipulaPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -eq 0) {
            LogInfo("Successfully generated JSON data for $questionnaireName")
        }
        else {
            LogWarning("Failed generating JSON data for $questionnaireName")
            LogWarning("Manipula exit code - $process.ExitCode")
        }
    }
    catch {
        LogWarning("Failed generating JSON data for $questionnaireName")
        LogWarning("$($_.Exception.Message)")
        return
    }

    # Move output to subfolder if specified
    if (-not [string]::IsNullOrEmpty($subFolder)) {
        LogInfo("Moving JSON data output to subfolder")
        Move-Item -Path "$processingFolder\*.json" -Destination $subFolder -Force -ErrorAction SilentlyContinue
        LogInfo("Moved JSON data output to subfolder")
    }
    else {
        LogInfo("JSON data output not moved to subfolder")
    }
}
