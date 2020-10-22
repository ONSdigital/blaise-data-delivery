using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services.Files;
using log4net;
using System;
using System.Linq;
using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace BlaiseDataDelivery.MessageHandlers
{
    public class DataDeliveryMessageHandler : IMessageHandler
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

        public bool HandleMessage(string message)
        {            
            try
            {
                //map the message taken off the queue to a model we can use
                var messageModel = _mapper.MapToMessageModel(message);

                //get a list of available files for data delivery
                var dataDeliveryFiles = _fileService.CreateDeliveryFiles(messageModel.ServerParkName, messageModel.InstrumentName, _configuration.LocalProcessFolder).ToList();

                if(!dataDeliveryFiles.Any())
                {
                    _logger.Info($"No files are available to deliver on server park {messageModel.ServerParkName}'");
                    return true;
                }

                _logger.Info($"There are '{dataDeliveryFiles.Count}' available files to deliver on server park {messageModel.ServerParkName}'");

                //create encrypted zip file 
                var encryptedZipFile = _fileService.CreateEncryptedZipFile(dataDeliveryFiles, messageModel);
                _logger.Info($"Encrypted files into the zip file '{encryptedZipFile}'");

                //upload the zip to bucket
                _fileService.UploadFileToBucket(encryptedZipFile, _configuration.BucketName);
                _logger.Info($"Uploaded the zip file '{encryptedZipFile}' to the bucket '{_configuration.BucketName}'");

                //clean up files
                _fileService.DeleteFile(encryptedZipFile);

               _logger.Info($"Cleaned up the source and temporary files");

                return true;
            }
            catch(Exception ex)
            {
                _logger.Error($"An exception occurred in processing message {message} - {ex.Message}");

                return false;
            }
        }
    }
}
