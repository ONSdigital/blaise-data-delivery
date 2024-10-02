. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\ConfigFunctions.ps1"
. "$PSScriptRoot\SpssFunctions.ps1"
. "$PSScriptRoot\XmlFunctions.ps1"
. "$PSScriptRoot\JsonFunctions.ps1"
. "$PSScriptRoot\AsciiFunctions.ps1"


function AddAdditionalFilesToDeliveryPackage {
    param(
        [string] $surveyType,
        [string] $processingFolder,
        [string] $deliveryZip,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $tempPath
    )
          
    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "No survey type argument provided"
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder file not found"
    }

    If (-not (Test-Path $deliveryZip)) {
        throw "$deliveryZip file not found"
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "No questionnaire name argument provided"
    }
          
    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType

    # Generate and add SPSS files if configured
    if($config.deliver.spss -eq $true) {
        LogInfo("Adding SPSS files")
        AddSpssFilesToDeliveryPackage -deliveryZip $deliveryZip -processingFolder $processingFolder -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath
    }

    # Generate and add Ascii files if configured
    if($config.deliver.ascii -eq $true) {
        LogInfo("Adding ASCII files")
        AddAsciiFilesToDeliveryPackage -deliveryZip $deliveryZip -processingFolder $processingFolder -questionnaireName $questionnaireName -subFolder $processingSubFolder -tempPath $tempPath
    }

    # Generate and add XML Files if configured
    if($config.deliver.xml -eq $true) {
        LogInfo("Adding XML files")
        AddXMLFileToDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryZip -questionnaireName $questionnaireName -subFolder $processingSubFolder -tempPath $tempPath
    }

    # Generate and add json Files if configured
    if($config.deliver.json -eq $true) {
        LogInfo("Adding JSON files")
        AddJSONFileToDeliveryPackage -processingFolder $processingFolder -deliveryZip $deliveryZip -questionnaireName $questionnaireName -subFolder $processingSubFolder -tempPath $tempPath
    }
}