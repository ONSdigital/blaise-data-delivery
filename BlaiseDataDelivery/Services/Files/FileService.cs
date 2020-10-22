using BlaiseDataDelivery.Interfaces.Services.Files;
using BlaiseDataDelivery.Models;
using System;
using System.Collections.Generic;
using Blaise.Nuget.Api.Contracts.Interfaces;

namespace BlaiseDataDelivery.Services.Files
{
    public class FileService : IFileService
    {
        private readonly IFileDirectoryService _directoryService;
        private readonly IFileEncryptionService _encryptionService;
        private readonly IFileZipService _zipService;
        private readonly IFileCloudStorageService _cloudStorageService;
        private readonly IBlaiseApi _blaiseApi;

        public FileService(
            IFileDirectoryService directoryService,
            IFileEncryptionService encryptionService,
            IFileZipService zipService,
            IFileCloudStorageService cloudStorageService,
            IBlaiseApi blaiseApi)
        {
            _directoryService = directoryService;
            _encryptionService = encryptionService;
            _zipService = zipService;
            _cloudStorageService = cloudStorageService;
            _blaiseApi = blaiseApi;
        }

        public IEnumerable<string> CreateDeliveryFiles(string serverParkName, string instrumentName,  string outputPath)
        {
            var files = new List<string>();
            files.Add(_blaiseApi.CreateDataDeliveryFile(_blaiseApi.GetDefaultConnectionModel(), serverParkName, instrumentName, outputPath));

            return files;
        }

        public string CreateEncryptedZipFile(IList<string> files, MessageModel messageModel)
        {
            var uniqueFileName = GenerateUniqueFileName(messageModel.InstrumentName, DateTime.Now);
            
            var tempZipFilePath = $"{messageModel.ServerParkName}\\Processed\\{uniqueFileName}.unencrypted.zip";
            _zipService.CreateZipFile(files, tempZipFilePath);
            
            var encryptedZipFilePath = $"{messageModel.ServerParkName}\\Processed\\{uniqueFileName}.zip";
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

        public void UploadFileToBucket(string zipFilePath, string bucketName)
        {
            _cloudStorageService.UploadFileToBucket(zipFilePath, bucketName);
        }

        public string GenerateUniqueFileName(string instrumentName, DateTime dateTime)
        {
            //generate a file name in the agreed format
            return $"dd_{instrumentName}_{dateTime:ddmmyyyy}_{dateTime:hhmmss}";
        }
    }
}
