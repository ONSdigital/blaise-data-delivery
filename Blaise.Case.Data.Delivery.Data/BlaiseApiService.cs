using System.Collections.Generic;
using System.Linq;
using Blaise.Case.Data.Delivery.Data.Interfaces;
using Blaise.Nuget.Api.Contracts.Interfaces;

namespace Blaise.Case.Data.Delivery.Data
{
   public class BlaiseApiService : IBlaiseApiService
    {
        private readonly IBlaiseApi _blaiseApi;

        public BlaiseApiService(IBlaiseApi blaiseApi)
        {
            _blaiseApi = blaiseApi;
        }

        public bool ServerParkExists(string serverParkName)
        {
            return _blaiseApi.ServerParkExists(_blaiseApi.GetDefaultConnectionModel(), serverParkName);
        }

        public IEnumerable<string> GetInstrumentsInstalled(string serverParkName)
        {
            var instruments = _blaiseApi.GetSurveys(_blaiseApi.GetDefaultConnectionModel(), serverParkName);

            return instruments.Select(i => i.Name);
        }

        public bool InstrumentExists(string serverParkName, string instrumentName)
        {
            return _blaiseApi.SurveyExists(_blaiseApi.GetDefaultConnectionModel(), instrumentName, serverParkName);
        }

        public void CreateDeliveryFiles(string serverParkName, string instrumentName, string outputPath)
        {
            _blaiseApi.BackupSurveyToFile(_blaiseApi.GetDefaultConnectionModel(), serverParkName, instrumentName, outputPath);
        }
    }
}
