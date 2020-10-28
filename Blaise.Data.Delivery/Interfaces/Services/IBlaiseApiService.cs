using System.Collections.Generic;

namespace Blaise.Data.Delivery.Interfaces.Services
{
    public interface IBlaiseApiService
    {
        bool ServerParkExists(string serverParkName);

        bool InstrumentExists(string serverParkName, string instrumentName);

        IEnumerable<string> GetInstrumentsInstalled(string serverParkName);
        
        void CreateDeliveryFiles(string serverParkName, string instrumentName, string outputPath);
    }
}