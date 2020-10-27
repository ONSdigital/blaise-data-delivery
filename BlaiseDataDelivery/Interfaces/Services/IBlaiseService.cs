using System.Collections.Generic;

namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IBlaiseService
    {
        bool ServerParkExists(string serverParkName);

        bool InstrumentExists(string serverParkName, string instrumentName);

        IEnumerable<string> GetInstrumentsInstalled(string serverParkName);
        
        void CreateDeliveryFiles(string serverParkName, string instrumentName, string outputPath);
    }
}