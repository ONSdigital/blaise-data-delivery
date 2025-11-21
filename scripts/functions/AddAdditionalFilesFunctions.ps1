. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\ConfigFunctions.ps1"
. "$PSScriptRoot\AsciiDataFunctions.ps1"
. "$PSScriptRoot\JsonDataFunctions.ps1"
. "$PSScriptRoot\SpssMetadataFunctions.ps1"
. "$PSScriptRoot\XmlDataFunctions.ps1"
. "$PSScriptRoot\XmlMetadataFunctions.ps1"

function AddAdditionalFilesToDeliveryPackage {
    param(
        [string] $surveyType,
        [string] $processingFolder,
        [string] $questionnaireName,
        [string] $subFolder
    )

    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "surveyType not provided"
    }

    If ([string]::IsNullOrEmpty($processingFolder)) {
        throw "processingFolder not provided"
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder not found" 
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "questionnaireName not provided"
    }

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType
    LogInfo("Add additional files config: $($config.deliver) $($config)")

    # Generate and add ASCII data files if configured
    if($config.deliver.asciiData -eq $true) {
        LogInfo("Adding ASCII data files")
        AddAsciiDataToDelivery -processingFolder $processingFolder -questionnaireName $questionnaireName -subFolder $subFolder
    }

    # Generate and add JSON data files if configured
    if($config.deliver.jsonData -eq $true) {
        LogInfo("Adding JSON data files")
        AddJsonDataToDelivery -processingFolder $processingFolder -questionnaireName $questionnaireName -subFolder $subFolder
    }

    # Generate and add SPSS metadata files if configured
    if($config.deliver.spssMetadata -eq $true) {
        LogInfo("Adding SPSS metadata files")
        AddSpssMetadataToDelivery -processingFolder $processingFolder -questionnaireName $questionnaireName -subFolder $subFolder
    }

    # Generate and add XML data files if configured
    if($config.deliver.xmlData -eq $true) {
        LogInfo("Adding XML data files")
        AddXmlDataToDelivery -processingFolder $processingFolder -questionnaireName $questionnaireName -subFolder $subFolder
    }

    # Generate and add XML metadata files if configured
    if($config.deliver.xmlMetadata -eq $true) {
        LogInfo("Adding XML metadata files")
        AddXmlMetadataToDelivery -processingFolder $processingFolder -questionnaireName $questionnaireName -subFolder $subFolder
    }
}
