using BlaiseDataDelivery.Interfaces.Services.Files;
using BlaiseDataDelivery.Models;
using System;
using System.Collections.Generic;

namespace BlaiseDataDelivery.Services.Files
{
    public class FileService : IFileService
    {
        private readonly IFileDirectoryService _directoryService;
        private readonly IFileEncryptionService _encryptionService;
        private readonly IFileZipService _zipService;
        private readonly IFileCloudStorageService _cloudStorageService;

        public FileService(
            IFileDirectoryService directoryService,
            IFileEncryptionService encryptionService,
            IFileZipService zipService,
            IFileCloudStorageService cloudStorageService)
        {
            _directoryService = directoryService;
            _encryptionService = encryptionService;
            _zipService = zipService;
            _cloudStorageService = cloudStorageService;
        }

        public string CreateEncryptedZipFile(IList<string> files, MessageModel messageModel)
        {
            var uniqueFileName = GenerateUniqueFileName(messageModel.InstrumentName, DateTime.Now);
            
            var tempZipFilePath = $"{messageModel.SourceFilePath}\\Processed\\{uniqueFileName}.unencrypted.zip";
            _zipService.CreateZipFile(files, tempZipFilePath);
            
            var encryptedZipFilePath = $"{messageModel.SourceFilePath}\\Processed\\{uniqueFileName}.zip";
            _encryptionService.EncryptFile(tempZipFilePath, encryptedZipFilePath);

            DeleteFile(tempZipFilePath);

            return encryptedZipFilePath;
        }

        public void DeleteFile(string filePath)
        {
            _directoryService.DeleteFile(filePath);
        }

        public void DeleteFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                _directoryService.DeleteFile(file);
            }
        }

        public IEnumerable<string> GetFiles(string path, string instrumentName, string filePattern)
        {
            return _directoryService.GetFiles(path, $"{instrumentName}{filePattern}");
        }

        public void UploadFileToBucket(string zipFilePath, string bucketName)
        {
            _cloudStorageService.UploadFileToBucket(zipFilePath, bucketName);
        }

        public string GenerateUniqueFileName(string instrumentName, DateTime dateTime)
        {
            //generate a file name in the agreed format
            return $"dd_{instrumentName}_{dateTime:ddMMyyyy}_{dateTime:HHmmss}";
        }
    }
}
