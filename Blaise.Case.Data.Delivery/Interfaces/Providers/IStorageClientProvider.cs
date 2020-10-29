
using Google.Cloud.Storage.V1;

namespace Blaise.Case.Data.Delivery.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        StorageClient GetStorageClient();

        void Dispose();
    }
}
