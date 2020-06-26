using Blaise.Queue.Contracts.Interfaces.MessageHandlers;
using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services.Files;
using log4net;
using System;
using System.Linq;

namespace BlaiseDataDelivery.MessageHandlers
{
    public class DataDeliveryMessageHandler : IMessageHandlerCallback
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configuration;
        private readonly IMessageModelMapper _mapper;
        private readonly IFileService _fileService;

        public DataDeliveryMessageHandler(
            ILog logger,
            IConfigurationProvider configuration,
            IMessageModelMapper mapper,
            IFileService fileService)
        {
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
            _fileService = fileService;
        }

        public bool HandleMessage(string messageType, string message)
        {            
            try
            {
                //map the message taken off the queue to a model we can use
                var messageModel = _mapper.MapToMessageModel(message);

                //get a list of available files for data delivery
                var filesToProcess = _fileService.GetFiles(messageModel.SourceFilePath, _configuration.FilePattern).ToList();


                //no files available - an error must have occured
                if(!filesToProcess.Any())
                {
                    _logger.Info($"No files are available to process in the path '{messageModel.SourceFilePath}' for the file pattern '{_configuration.FilePattern}'");
                    return true;
                }

                //create encrypted zip file
                _logger.Info($"Attempting to create zip file for files '{string.Join(",", filesToProcess.ToArray())}'");
                var encryptedZipFile = _fileService.CreateEncryptedZipFile(filesToProcess, messageModel);
                _logger.Info($"Created encrypted zip file {encryptedZipFile}");

                //upload the zip to bucket
                _logger.Info($"Attempting to upload zip file '{encryptedZipFile}' to bucket '{_configuration.BucketName}'");
                _fileService.UploadFileToBucket(encryptedZipFile, _configuration.BucketName);
                _logger.Info($"Uploaded zip file '{encryptedZipFile}' to bucket '{_configuration.BucketName}'");

                //clean up files
                _fileService.DeleteFile(encryptedZipFile);
               _fileService.DeleteFiles(filesToProcess);

            }
            catch(Exception ex)
            {
                _logger.Error($"An exception occured in processing message '{message}' with exception {ex.Message} and inner exception '{ex.InnerException}'");
            }

            return true;
        }
    }
}
