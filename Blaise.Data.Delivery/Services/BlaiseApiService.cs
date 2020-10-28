using System.Collections.Generic;
using System.Linq;
using Blaise.Data.Delivery.Interfaces.Services;
using Blaise.Nuget.Api.Contracts.Interfaces;
using log4net;

namespace Blaise.Data.Delivery.Services
{
   public class BlaiseApiService : IBlaiseApiService
    {
        private readonly IBlaiseApi _blaiseApi;
        private readonly ILog _logger;

        public BlaiseApiService(
            IBlaiseApi blaiseApi, 
            ILog logger)
        {
            _blaiseApi = blaiseApi;
            _logger = logger;
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
            _logger.Info($"Created delivery file for instrument '{instrumentName}'");

            _blaiseApi.BackupSurveyToFile(_blaiseApi.GetDefaultConnectionModel(), serverParkName, instrumentName, outputPath);
        }
    }
}
