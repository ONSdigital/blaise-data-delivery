. "$PSScriptRoot\LoggingFunctions.ps1"
. "$PSScriptRoot\ConfigFunctions.ps1"
. "$PSScriptRoot\CloudFunctions.ps1"
. "$PSScriptRoot\ManipulaFunctions.ps1"
. "$PSScriptRoot\FileFunctions.ps1"
. "$PSScriptRoot\AddAdditionalFilesFunctions.ps1"

function CreateDeliveryFile {
    param (
        [string] $deliveryFile,
        [string] $serverParkName,
        [string] $surveyType,
        [string] $questionnaireName,
        [string] $subFolder,
        [string] $dqsBucket,
        [string] $processingPath,
        [string[]] $keepQuestionnairePackageFileExtensions = @('bdix', 'bmix', 'bdbx', 'blix', 'csv'),
        [string[]] $deliverFileExtensions = @('bdix', 'bmix', 'bdbx', 'blix', 'sps', 'fps', 'asc', 'json', 'xml', 'csv')
    )

    If ([string]::IsNullOrEmpty($deliveryFile)) {
        throw "deliveryFile not provided"
    }

    If ([string]::IsNullOrEmpty($serverParkName)) {
        throw "serverParkName not provided"
    }

    If ([string]::IsNullOrEmpty($surveyType)) {
        throw "surveyType not provided"
    }

    If ([string]::IsNullOrEmpty($questionnaireName)) {
        throw "questionnaireName not provided"
    }

    If ([string]::IsNullOrEmpty($dqsBucket)) {
        throw "dqsBucket not provided"
    }

    If ([string]::IsNullOrEmpty($processingPath)) {
        throw "processingPath not provided"
    }

    # Get configuration for survey type
    $config = GetConfigFromFile -surveyType $surveyType

    # Create a temporary folder for processing questionnaire data delivery
    $processingFolderPath = CreateFolder -folderPath $processingPath -folderName "$($questionnaireName)_$(Get-Date -format "ddMMyyyy")_$(Get-Date -format "HHmmss")"
    
    # Download questionnaire package
    LogInfo("Downloading questionnaire package $questionnaireName from $dqsBucket bucket")
    DownloadFileFromBucket -questionnaireFileName "$($questionnaireName).bpkg" -bucketName $dqsBucket -filePath $deliveryFile
 
    # Extract data to Blaise file format (BDBX, BMIX, BDIX) via Blaise CLI, '2>&1' redirects output to command line allowing errors to bubble up
    LogInfo("Extracting data in Blaise format for questionnaire $questionnaireName via Blaise CLI")
    C:\BlaiseServices\BlaiseCli\blaise.cli datadelivery -s $serverParkName -q $questionnaireName -f $deliveryFile -a $config.auditTrailData -b $config.batchSize 2>&1        

    # Extact questionnaire package to processing folder, now contains data in Blaise file format from the previous step
    LogInfo("Extracting questionnaire package $deliveryFile to processing folder $processingFolderPath")
    ExtractZipFile -pathTo7zip $processingPath -zipFilePath $deliveryFile -destinationPath $processingFolderPath

    # Remove unwanted files after questionnaire package extraction
    Get-ChildItem -Path $processingFolderPath -Recurse -File | Where-Object {
        $ext = $_.Extension.TrimStart('.').ToLower()
        -not ($keepQuestionnairePackageFileExtensions -contains $ext)
        } | Remove-Item -Force
    Get-ChildItem -Path $processingFolderPath -Recurse -Directory | Sort-Object -Property FullName -Descending | Where-Object {
        @(Get-ChildItem -Path $_.FullName -Recurse | Where-Object { -not $_.PSIsContainer }).Count -eq 0
        } | Remove-Item -Force
        
    # Determine subfolder creation preference for non-Blaise file formats
    $processingSubFolderPath = $null
    if($config.createSubFolder -eq $true) {
        LogInfo("Creating subfolder for $questionnaireName data delivery")
    
        # Get the unique name of the processing folder to use for the subfolder
        $subFolderName = Split-Path $processingFolderPath -Leaf
    
        # Create subfolder within the processing folder
        $processingSubFolderPath = CreateFolder -folderPath $processingFolderPath -folderName $subFolderName
    }
    else {
        LogInfo("Did not create subfolder for $questionnaireName data delivery")
    }

    # Add Manipula files to the processing folder
    LogInfo("Adding Manipula binaries to $processingFolderPath")
    AddManipulaToProcessingFolder -manipulaPackage "$processingPath/manipula.zip" -processingFolder $processingFolderPath -processingPath $processingPath

    # Add additional file formats specified in the survey config, will also be placed in the processing subfolder if config.createSubFolder is true, i.e. processingSubFolderPath is not $NULL
    LogInfo("Adding additional file formats to $processingFolderPath")
    AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -processingFolder $processingFolderPath -questionnaireName $questionnaireName -subFolder $processingSubFolderPath
    if ($config.deliver.asciiData -or $config.deliver.jsonData -or $config.deliver.spssMetadata -or $config.deliver.xmlData -or $config.deliver.xmlMetadata) {
        LogInfo("Adding Manipula binaries to $processingFolderPath")
        AddManipulaToProcessingFolder -manipulaPackage "$processingPath/manipula.zip" -processingFolder $processingFolderPath -processingPath $processingPath
        AddAdditionalFilesToDeliveryPackage -surveyType $surveyType -processingFolder $processingFolderPath -questionnaireName $questionnaireName -subFolder $processingSubFolderPath
    }

    # Remove files we don't want delivered
    Get-ChildItem -Path $processingFolderPath -Recurse -File | Where-Object {
        $_.Name -like '*$$$*' -or
        $_.Name -like '*.manx.bmix' -or
        $_.Name -like '*.asc.bdix' -or
        $_.Name -like '*.json.bdix' -or
        $_.Name -like '*.xml.bdix' -or
        $_.Name -like '*_sql.bdix'
    } | Remove-Item -Force
    Get-ChildItem -Path $processingFolderPath -Recurse -File | Where-Object {
        $ext = $_.Extension.TrimStart('.').ToLower()
        -not ($deliverFileExtensions -contains $ext)
    } | Remove-Item -Force

    # Create the data delivery zip from the contents of the processing folder
    LogInfo("Creating data delivery zip $deliveryFile")
    Remove-Item -Path $deliveryFile -Force -ErrorAction SilentlyContinue
    AddFilesToZip -pathTo7zip $processingPath -files "$processingFolderPath\*" -zipFilePath $deliveryFile
    LogInfo("Successfully created data delivery zip $deliveryFile")
}
