using BlaiseDataDelivery.Helpers;
using BlaiseDataDelivery.Interfaces.Services.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BlaiseDataDelivery.Services.Files
{
    public class FileDirectoryService : IFileDirectoryService
    {
        public void DeleteFile(string filePath)
        {
            filePath.ThrowExceptionIfNullOrEmpty("filePath");

            File.Delete(filePath);
        }

        public IEnumerable<string> GetFiles(string path, string filePattern)
        {
            path.ThrowExceptionIfNullOrEmpty("path");
            filePattern.ThrowExceptionIfNullOrEmpty("filePattern");

            var directory = GetDirectory(path);
            var files = directory.GetFiles(filePattern);

            return files.Select(f => f.FullName);
        }

        public void MoveFiles(IList<string> files, string destinationFilePath)
        {
            if (!files.Any())
            {
                throw new ArgumentException($"No files provided");
            }

            destinationFilePath.ThrowExceptionIfNullOrEmpty("destinationFilePath");


            foreach (var fileToMove in files)
            {
                File.Move(fileToMove, destinationFilePath);
            }
        }

        private DirectoryInfo GetDirectory(string path)
        {
            var directory = new DirectoryInfo(path);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"The dirctory '{path}' was not found");
            }

            return directory;
        }
    }
}
