namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IDeliveryService
    {
        void UploadInstrumentFileToBucket(string filePath, string instrumentName, string bucketName);
    }
}
