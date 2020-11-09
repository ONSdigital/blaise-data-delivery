
namespace Blaise.Case.Data.Delivery.Core.Interfaces
{
    public interface IStorageService
    {
        void UploadFileToBucket(string filePath, string bucketName);
    }
}
