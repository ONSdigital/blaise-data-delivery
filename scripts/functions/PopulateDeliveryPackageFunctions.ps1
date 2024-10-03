. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\ConfigFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
. "$PSScriptRoot\AddAdditionalFilesFunctions.ps1"

function PopulateDeliveryPackage {
    param (
        [string] $serverParkName,
        [string] $surveyType,
        [string] $processingFolder,
        [string] $deliveryFile,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $tempPath        
    )  

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType

    # Populate data
    # the use of the parameter '2>&1' redirects output of the cli to the command line and will allow any errors to bubble up
    C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $serverParkName -q $questionnaireName -f $deliveryFile -a $config.auditTrailData -b $config.batchSize 2>&1        
    
    # Extact Questionnaire Package to processing folder
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $deliveryFile -destinationPath $processingFolder

    # Add additional file formats specified in the config i.e. JSON, ASCII
    AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -deliveryFile $deliveryFile -processingFolder $processingFolder -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath   
}

function PopulateUneditedDeliveryPackage {
    param (
        [string] $serverParkName,
        [string] $surveyType,
        [string] $processingFolder,
        [string] $deliveryFile,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $tempPath        
    )  

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType

    # Rename bmix and bdix beeded to extract data from the unedited table
    RenameQuestionnaireFiles -tempPath $tempPath -processingFolder $processingFolder -deliveryFile $deliveryFile -questionnaireNameFrom $questionnaireName -questionnaireNameTo "$($questionnaireName)_UNEDITED"
    LogInfo("Renamed questionnaire files")

    # Populate data
    # The use of the parameter '2>&1' redirects output of the cli to the command line and will allow any errors to bubble up   
    LogInfo("Populate data")    
    & C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $serverParkName -q "$($questionnaireName)_UNEDITED" -f $deliveryFile -a $config.auditTrailData -b $config.batchSize 2>&1    
       
    # Rename bmix and bdix beeded to extract data from the unedited table
    RenameQuestionnaireFiles -tempPath $tempPath -processingFolder $processingFolder -deliveryFile $deliveryFile -questionnaireNameFrom "$($questionnaireName)_UNEDITED" -questionnaireNameTo $questionnaireName
    
    # Extact Questionnaire Package to processing folder
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $deliveryFile -destinationPath "$processingFolder"       

    # Add additional file formats specified in the config i.e. JSON, ASCII    
    AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -deliveryFile $deliveryFile -processingFolder $processingFolder -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath   
}