using Blaise.Data.Delivery.Interfaces.Providers;
using Google.Cloud.Storage.V1;

namespace Blaise.Data.Delivery.Providers
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
