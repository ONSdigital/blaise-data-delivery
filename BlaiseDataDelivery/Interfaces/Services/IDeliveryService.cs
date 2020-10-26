namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IDeliveryService
    {
        bool DeliverSingleInstrument(string serverParkName, string instrumentName, string tempFilePath,
            string bucketName);

        bool DeliverAllInstruments(string serverParkName, string tempFilePath, string bucketName);
    }
}
