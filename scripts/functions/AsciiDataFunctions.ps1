. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function AddAsciiDataToDelivery {
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

    # Copy Manipula ASCII data scripts to processing folder
    Copy-Item -Path "$PSScriptRoot\..\manipula\AsciiData\*" -Destination $processingFolder -Force

    # Generate ASCII data (also generates field property "remarks" FPS file)
    try {
        $manipulaPath = Join-Path $processingFolder "Manipula.exe"
        $msuxPath = Join-Path $processingFolder "GenerateAsciiData.msux"
        $bmixPath = Join-Path $processingFolder "$questionnaireName.bmix"
        $bdbxPath = Join-Path $processingFolder "$questionnaireName.bdbx"
        $outputPath = Join-Path $processingFolder "$questionnaireName.asc"
        $arguments = @(
            "`"$msuxPath`"",
            "-K:Meta=`"$bmixPath`"",
            "-I:`"$bdbxPath`"",
            "-O:`"$outputPath`"",
            "-Q:True"
        )
        $process = Start-Process -FilePath $manipulaPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -eq 0) {
            LogInfo("Successfully generated ASCII data for $questionnaireName")
        }
        else {
            LogWarning("Failed generating ASCII data for $questionnaireName")
            LogWarning("Manipula exit code - $process.ExitCode")
        }
    }
    catch {
        LogWarning("Failed generating ASCII data for $questionnaireName")
        LogWarning("$($_.Exception.Message)")
        return
    }

    # Move output to subfolder if specified
    if (-not [string]::IsNullOrEmpty($subFolder)) {
        LogInfo("Moving ASCII data output to subfolder")
        Move-Item -Path "$processingFolder\*.asc", "$processingFolder\*.fps" -Destination $subFolder -Force -ErrorAction SilentlyContinue
        LogInfo("Moved ASCII data output to subfolder")
    }
    else {
        LogInfo("ASCII data output not moved to subfolder")
    }
}
