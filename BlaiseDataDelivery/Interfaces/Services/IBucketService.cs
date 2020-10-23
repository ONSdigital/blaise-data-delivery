
namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IBucketService
    {
        void UploadFileToBucket(string filePath, string bucketName);
    }
}
