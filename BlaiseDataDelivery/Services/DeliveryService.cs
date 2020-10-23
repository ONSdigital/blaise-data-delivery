using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using BlaiseDataDelivery.Interfaces.Services;
using log4net;

namespace BlaiseDataDelivery.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ICompressionService _compressionService;
        private readonly IBucketService _bucketService;
        private readonly ILog _logger;
        private readonly IFileSystem _fileSystem;

        public DeliveryService(
            IEncryptionService encryptionService,
            ICompressionService compressionService,
            IBucketService bucketService,
            ILog logger, 
            IFileSystem fileSystem)
        {
            _encryptionService = encryptionService;
            _compressionService = compressionService;
            _bucketService = bucketService;
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public void UploadInstrumentFileToBucket(string filePath, string instrumentName, string bucketName)
        {
            var encryptedZipFile = CreateEncryptedZipFile(new List<string>{ filePath }, instrumentName);
            _logger.Info($"Encrypted files into the zip file '{encryptedZipFile}'");

            //upload the zip to bucket
            UploadFileToBucket(encryptedZipFile, bucketName);
            _logger.Info($"Uploaded the zip file '{encryptedZipFile}' to the bucket '{bucketName}'");

            //clean up files
            DeleteFile(encryptedZipFile);
            DeleteFile(filePath);
            _logger.Info("Cleaned up the temporary files");
        }

        public string GenerateUniqueFileName(string instrumentName, DateTime dateTime)
        {
            //generate a file name in the agreed format
            return $"dd_{instrumentName}_{dateTime:ddmmyyyy}_{dateTime:hhmmss}";
        }

        public string CreateEncryptedZipFile(IList<string> files, string instrumentName)
        {
            var uniqueFileName = GenerateUniqueFileName(instrumentName, DateTime.Now);
            
            var tempZipFilePath = $"{uniqueFileName}.unencrypted.zip";
            _compressionService.CreateZipFile(files, tempZipFilePath);
            
            var encryptedZipFilePath = $"{uniqueFileName}.zip";
            _encryptionService.EncryptFile(tempZipFilePath, encryptedZipFilePath);

            DeleteFile(tempZipFilePath);

            return encryptedZipFilePath;
        }

        public void UploadFileToBucket(string zipFilePath, string bucketName)
        {
            _bucketService.UploadFileToBucket(zipFilePath, bucketName);
        }

        public void DeleteFile(string filePath)
        {
            _fileSystem.File.Delete(filePath);
        }

        public void DeleteFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                _fileSystem.File.Delete(file);
            }
        }
    }
}
