
namespace Blaise.Case.Data.Delivery.CloudStorage.Interfaces
{
    public interface IStorageService
    {
        void UploadFileToBucket(string filePath, string bucketName);
    }
}
