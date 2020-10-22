using BlaiseDataDelivery.Models;
using System.Collections.Generic;

namespace BlaiseDataDelivery.Interfaces.Services.Files
{
    public interface IFileService
    {
        IEnumerable<string> CreateDeliveryFiles(string instrumentName, string serverParkName, string outputPath);
        string CreateEncryptedZipFile(IList<string> files, MessageModel messageModel);

        void UploadFileToBucket(string zipFilePath, string bucketName);
        void DeleteFiles(IEnumerable<string> files);
        void DeleteFile(string filePath);
    }
}
