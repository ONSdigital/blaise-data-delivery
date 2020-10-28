using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Blaise.Data.Delivery.Interfaces.Services;

namespace Blaise.Data.Delivery.Services
{
    public class FileService : IFileService
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ICompressionService _compressionService;
        private readonly IFileSystem _fileSystem;

        public FileService(
            IEncryptionService encryptionService, 
            ICompressionService compressionService, 
            IFileSystem fileSystem)
        {
            _encryptionService = encryptionService;
            _compressionService = compressionService;
            _fileSystem = fileSystem;
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

        public IEnumerable<string> GetFiles(string filePath)
        {
            return _fileSystem.Directory.GetFiles(filePath);
        }

        public string GenerateUniqueFileName(string instrumentName, DateTime dateTime)
        {
            //generate a file name in the agreed format
            return $"dd_{instrumentName}_{dateTime:ddmmyyyy}_{dateTime:hhmmss}";
        }


    }
}
