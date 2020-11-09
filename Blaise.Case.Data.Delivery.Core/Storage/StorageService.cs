using System.IO;
using Blaise.Case.Data.Delivery.Core.Interfaces;

namespace Blaise.Case.Data.Delivery.Core.Storage
{
    public class StorageService : IStorageService
    {
        private readonly IStorageClientProvider _storageClient;

        public StorageService(IStorageClientProvider storageClient)
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

            _storageClient.Dispose();
        }
    }
}
