
namespace Blaise.Case.Data.Delivery.Interfaces.Services
{
    public interface IBucketService
    {
        void UploadFileToBucket(string filePath, string bucketName);
    }
}
