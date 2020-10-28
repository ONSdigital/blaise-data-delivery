using System.IO;
using Blaise.Data.Delivery.Interfaces.Providers;
using Blaise.Data.Delivery.Interfaces.Services;

namespace Blaise.Data.Delivery.Services
{
    public class BucketService : IBucketService
    {
        private readonly IStorageClientProvider _storageClient;

        public BucketService(IStorageClientProvider storageClient)
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
