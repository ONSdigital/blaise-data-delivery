using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services.Files;
using System.IO;

namespace BlaiseDataDelivery.Services.Files
{
    public class FileCloudStorageService : IFileCloudStorageService
    {
        private readonly IStorageClientProvider _storageClient;

        public FileCloudStorageService(IStorageClientProvider storageClient)
        {
            _storageClient = storageClient;
        }

        public void UploadFileToBucket(string filePath, string bucketName)
        {
            var fileName = Path.GetFileName(filePath);
            var bucket = _storageClient.GetStorageClient();

            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                bucket.UploadObject(bucketName, fileName, null, streamWriter.BaseStream);
            }
        }
    }
}
