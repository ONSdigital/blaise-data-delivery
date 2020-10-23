using System.Collections.Generic;
using System.Linq;
using Blaise.Nuget.Api.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Services;
using log4net;

namespace BlaiseDataDelivery.Services
{
   public class BlaiseService : IBlaiseService
    {
        private readonly IBlaiseApi _blaiseApi;
        private readonly ILog _logger;

        public BlaiseService(
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
            return true;
        }

        public string CreateDeliveryFile(string serverParkName, string instrumentName, string outputPath)
        {
            _logger.Info($"Created delivery file for instrument '{instrumentName}'");
            return _blaiseApi.CreateDataDeliveryFile(_blaiseApi.GetDefaultConnectionModel(), serverParkName, instrumentName, outputPath);
        }

        public IEnumerable<string> CreateDeliveryFiles(string serverParkName, string outputPath)
        {
            var deliveryFiles = new List<string>();
            var instruments = _blaiseApi.GetSurveys(_blaiseApi.GetDefaultConnectionModel(), serverParkName).ToList();

            if (!instruments.Any())
            {
                _logger.Warn($"There are no surveys installed on server park '{serverParkName}'");
                return deliveryFiles;
            }

            foreach (var instrument in instruments)
            {
                deliveryFiles.Add(CreateDeliveryFile(serverParkName, instrument.Name, outputPath));
            }
            
            return deliveryFiles;
        }
    }
}
