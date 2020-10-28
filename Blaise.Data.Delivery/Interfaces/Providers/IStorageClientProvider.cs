
using Google.Cloud.Storage.V1;

namespace Blaise.Data.Delivery.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        StorageClient GetStorageClient();

        void Dispose();
    }
}
