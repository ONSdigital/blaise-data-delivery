. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\ConfigFunctions.ps1"
. "$PSScriptRoot\CloudFunctions.ps1"
. "$PSScriptRoot\PopulateDeliveryPackageFunctions.ps1"
. "$PSScriptRoot\ManipulaFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"

function CreateDeliveryFile {
    param (
        [string] $deliveryFile,
        [string] $serverParkName,
        [string] $surveyType,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $tempPath, 
        [bool] $uneditedData=$false       
    )

    If ([string]::IsNullOrEmpty($deliveryFile)) {
        throw "No deliveryFile argument provided"
    }

    If ([string]::IsNullOrEmpty($serverParkName)) {
        throw "No serverParkName argument provided"
    }

    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "No surveyType argument provided"
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "No questionnaire name argument provided"
    }

    If ([string]::IsNullOrEmpty($dqsBucket)) {
        throw "No dqsBucket argument provided"
    }

    If ([string]::IsNullOrEmpty($tempPath)) {
        throw "No tempPath argument provided"
    }

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType
    
    # Download questionnaire package
    LogInfo("Download questionnaire package")
    DownloadFileFromBucket -questionnaireFileName "$($questionnaireName).bpkg" -bucketName $dqsBucket -filePath $deliveryFile
    
    # Create a temporary folder for processing questionnaires
    $processingFolder = CreateANewFolder -folderPath $tempPath -folderName "$($questionnaireName)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"
     
    # If we need to use subfolders then create one and set variable
    if($config.createSubFolder -eq $true) {
        LogInfo("Creating subfolder for delivery")
    
        # Gets the folder name of the processing folder
        $processingSubFolderName = GetFolderNameFromAPath -folderPath $processingFolder
    
        # Create a sub folder within the temporary folder
        $processingSubFolder = CreateANewFolder -folderPath $processingFolder -folderName $processingSubFolderName
    }
    else {
        # This variable will be ignored in the function called if passed - ugh
        LogInfo("Did not create subfolder for delivery")
        $processingSubFolder = $NULL
    }

    # Add manipula and questionnaire package to processing folder
    LogInfo("Add manipula")
    AddManipulaToProcessingFolder -manipulaPackage "$tempPath/manipula.zip" -processingFolder $processingFolder -tempPath $tempPath
 
    # Populate data files and formats
    if($uneditedData -eq $false) {
        PopulateDeliveryPackage -serverParkName $serverParkName -surveyType $surveyType deliveryFile $deliveryFile -processingFolder $processingFolder -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath    
    }
    else {
        PopulateUneditedDeliveryPackage -serverParkName $serverParkName -surveyType $surveyType deliveryFile $deliveryFile -processingFolder $processingFolder -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath    
    }

}