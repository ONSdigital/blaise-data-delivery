
using Google.Cloud.Storage.V1;

namespace Blaise.Case.Data.Delivery.Core.Interfaces
{
    public interface IStorageClientProvider
    {
        StorageClient GetStorageClient();

        void Dispose();
    }
}
