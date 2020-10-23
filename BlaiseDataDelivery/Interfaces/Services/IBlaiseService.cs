using System.Collections.Generic;

namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IBlaiseService
    {
        bool ServerParkExists(string serverParkName);

        bool InstrumentExists(string serverParkName, string instrumentName);

        IEnumerable<string> GetInstrumentsInstalled(string serverParkName);


        string CreateDeliveryFile(string serverParkName, string instrumentName, string outputPath);

        IEnumerable<string> CreateDeliveryFiles(string serverParkName, string outputPath);
    }
}