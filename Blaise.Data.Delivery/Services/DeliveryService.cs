using System.Linq;
using Blaise.Data.Delivery.Interfaces.Services;
using log4net;

namespace Blaise.Data.Delivery.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly ILog _logger;
        private readonly IBucketService _bucketService;
        private readonly IFileService _fileService;
        private readonly IBlaiseApiService _blaiseService;

        public DeliveryService(
            ILog logger,
            IBucketService bucketService,
            IFileService fileService, 
            IBlaiseApiService blaiseService)
        {
            _logger = logger;
            _bucketService = bucketService;
            _fileService = fileService;
            _blaiseService = blaiseService;
        }

        public bool DeliverSingleInstrument(string serverParkName, string instrumentName, string tempFilePath, string bucketName)
        {
            if (!_blaiseService.InstrumentExists(serverParkName, instrumentName))
            {
                _logger.Error($"The instrument '{instrumentName}' does not exist on server park '{serverParkName}'");
                return false;
            }

            DeliverInstrument(serverParkName, instrumentName, tempFilePath, bucketName);
            return true;
        }

        public bool DeliverAllInstruments(string serverParkName, string tempFilePath, string bucketName)
        {
            var instrumentsInstalled = _blaiseService.GetInstrumentsInstalled(serverParkName).ToList();

            if (!instrumentsInstalled.Any())
            {
                _logger.Error($"The server park '{serverParkName}' does not have any instruments installed");
                return false;
            }

            foreach (var instrument in instrumentsInstalled)
            {
                DeliverInstrument(serverParkName, instrument, tempFilePath, bucketName);
            }

            return true;
        }

        public void UploadInstrumentFilesToBucket(string filePath, string instrumentName, string bucketName)
        {
            _logger.Info($"Getting files from the path '{filePath}'");
            var instrumentFiles = _fileService.GetFiles(filePath).ToList();

            _logger.Info($"Retrieved {instrumentFiles.Count} files from the path '{filePath}'");

            var encryptedZipFile = _fileService.CreateEncryptedZipFile(instrumentFiles, instrumentName,
                filePath);
            _logger.Info($"Encrypted files into the zip file '{encryptedZipFile}'");

            //upload the zip to bucket
            UploadFileToBucket(encryptedZipFile, bucketName);
            _logger.Info($"Uploaded the zip file '{encryptedZipFile}' to the bucket '{bucketName}'");

            //clean up files
            _fileService.DeleteFile(encryptedZipFile);
            _fileService.DeleteFiles(instrumentFiles);
            _logger.Info("Cleaned up the temporary files");
        }

        private void UploadFileToBucket(string zipFilePath, string bucketName)
        {
            _bucketService.UploadFileToBucket(zipFilePath, bucketName);
        }

        private void DeliverInstrument(string serverParkName, string instrumentName, string tempFilePath, string bucketName)
        {
            var outputPath = $"{tempFilePath}\\{instrumentName}";

            _blaiseService.CreateDeliveryFiles(serverParkName, instrumentName, outputPath);
            UploadInstrumentFilesToBucket(outputPath, instrumentName, bucketName);
        }
    }
}
