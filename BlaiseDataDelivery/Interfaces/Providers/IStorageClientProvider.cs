
using Google.Cloud.Storage.V1;

namespace BlaiseDataDelivery.Interfaces.Providers
{
    public interface IStorageClientProvider
    {
        StorageClient GetStorageClient();

        void Dispose();
    }
}
