. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\ConfigFunctions.ps1"
. "$PSScriptRoot\AddAdditionalFilesToDeliveryPackage.ps1"


function PopulateDeliveryPackage {
    param(
        [string] $serverParkName,
        [string] $surveyType,
        [string] $processingFolder,
        [string] $deliveryFile,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $tempPath
    )
        
    If ([string]::IsNullOrEmpty($serverParkName)) {
        throw "No serverParkName argument provided"
    }

    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "No surveyType argument provided"
    }

    If ([string]::IsNullOrEmpty($processingFolder)) {
        throw "No processingFolder argument provided"
    }

    If (-not (Test-Path $processingFolder)) {
        throw "$processingFolder file not found"
    }

    If ([string]::IsNullOrEmpty($deliveryFile)) {
        throw "No deliveryFile argument provided"
    }

    If (-not (Test-Path $deliveryFile)) {
        throw "$deliveryFile file not found"
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

    if($config.hasEditMode -eq $false) {
        PopulateDeliveryPackage 
            -serverParkName $serverParkName 
            -surveyType $surveyType
            -processingFolder $processingFolder
            -deliveryFile $deliveryFile
            -questionnaireName $questionnaireName
            -subFolder $subFolder
            -dqsBucket $dqsBucket
            -tempPath $tempPath
    }
    else {
        PopulateDeliveryPackageWithEditing 
            -serverParkName $serverParkName 
            -surveyType $surveyType
            -processingFolder $processingFolder
            -deliveryFile $deliveryFile
            -questionnaireName $questionnaireName
            -subFolder $subFolder
            -dqsBucket $dqsBucket
            -tempPath $tempPath
    }
}

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
    LogInfo("Add AddAdditionalFilesToDeliveryPackage")
    AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -deliveryFile $deliveryFile -processingFolder $processingFolder -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath   
}

function PopulateDeliveryPackageWithEditing {
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
    $editedDataFile = "$processingFolder\$($questionnaireName)_EDITED.zip"
    $uneditedDataFile = "$processingFolder$($questionnaireName)_UNEDITED.zip"

    Copy-Item $deliveryFile -Destination $editedDataFile
    Copy-Item $deliveryFile -Destination $uneditedDataFile

    # EDITED

    # Populate data
    # The use of the parameter '2>&1' redirects output of the cli to the command line and will allow any errors to bubble up           
    C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $serverParkName -q $questionnaireName -f $editedDataFile -a $config.auditTrailData -b $config.batchSize 2>&1            
    
    # Extact Questionnaire Package to processing folder
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $editedDataFile -destinationPath "$processingFolder\EDITED"   
    
    # Add additional file formats specified in the config i.e. JSON, ASCII
    #AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -deliveryFile $editedDataFile -processingFolder "$processingFolder\EDITED"  -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath   


    # UNEDITED

    # Rename bmix and bdix beeded to extract data from the unedited table
    RenameQuestionnaireFiles -tempPath $tempPath -processingFolder $processingFolder -deliveryFile $uneditedDataFile -questionnaireNameFrom $questionnaireName -questionnaireNameTo "$($questionnaireName)_UNEDITED"

    # Populate data
    # The use of the parameter '2>&1' redirects output of the cli to the command line and will allow any errors to bubble up       
    C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $serverParkName -q "$($questionnaireName)_UNEDITED" -f $uneditedDataFile -a false -b $config.batchSize 2>&1    
       
    # Rename bmix and bdix beeded to extract data from the unedited table
    RenameQuestionnaireFiles -tempPath $tempPath -processingFolder $processingFolder -deliveryFile $uneditedDataFile -questionnaireNameFrom "$($questionnaireName)_UNEDITED" -questionnaireNameTo $questionnaireName
    
    # Extact Questionnaire Package to processing folder
    ExtractZipFile -pathTo7zip $tempPath -zipFilePath $uneditedDataFile -destinationPath "$processingFolder\UNEDITED"       

    # Add additional file formats specified in the config i.e. JSON, ASCII    
    #AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -deliveryFile $uneditedDataFile -processingFolder "$processingFolder\UNEDITED" -questionnaireName $questionnaireName -dqsBucket $dqsBucket -subFolder $processingSubFolder -tempPath $tempPath   


    # Update delivery file with edited and unedited file
    UpdateDeliveryPackageFiles -tempPath $tempPath -processingFolder $processingFolder -deliveryFile $deliveryFile -editedDataFile $editedDataFile -uneditedDataFile $uneditedDataFile

}