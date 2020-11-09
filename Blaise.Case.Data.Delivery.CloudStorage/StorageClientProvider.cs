using Blaise.Case.Data.Delivery.CloudStorage.Interfaces;
using Google.Cloud.Storage.V1;

namespace Blaise.Case.Data.Delivery.CloudStorage
{
    public class StorageClientProvider : IStorageClientProvider
    {
        private StorageClient _storageClient;

        public StorageClient GetStorageClient()
        {
            var client = _storageClient;

            if (client != null)
            {
                return client;
            }

            return (_storageClient = StorageClient.Create());
        }
        public void Dispose()
        {
            _storageClient?.Dispose();
            _storageClient = null;
        }
    }
}
