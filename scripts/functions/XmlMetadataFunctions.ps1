. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function AddXmlMetadataToDelivery {
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

    # Generate XML metadata
    try {
        $metaViewerPath = Join-Path $processingFolder "MetaViewer.exe"
        $bmixPath = Join-Path $processingFolder "$questionnaireName.bmix"
        $arguments = @(
            "-F:`"$bmixPath`"",
            "-Export"
        )
        $process = Start-Process -FilePath $metaViewerPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        if ($process.ExitCode -eq 0) {
            LogInfo("Successfully generated XML metadata for $questionnaireName")
        }
        else {
            LogWarning("Failed generating XML metadata for $questionnaireName")
            LogWarning("MetaViewer exit code - $process.ExitCode")
        }
    }
    catch {
        LogWarning("Failed generating XML metadata for $questionnaireName")
        LogWarning("$($_.Exception.Message)")
        return
    }

    # Move output to subfolder if specified
    if (-not [string]::IsNullOrEmpty($subFolder)) {
        LogInfo("Moving XML metadata output to subfolder")
        Move-Item -Path "$processingFolder\*.xml" -Destination $subFolder -Force -ErrorAction SilentlyContinue
        LogInfo("Moved XML metadata output to subfolder")
    }
    else {
        LogInfo("XML metadata output not moved to subfolder")
    }
}
