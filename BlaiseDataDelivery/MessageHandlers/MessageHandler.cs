using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Providers;
using log4net;
using System;
using System.Linq;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Services;

namespace BlaiseDataDelivery.MessageHandlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configuration;
        private readonly IMessageModelMapper _mapper;
        private readonly IDeliveryService _deliveryService;
        private readonly IBlaiseService _blaiseService;

        public MessageHandler(
            ILog logger,
            IConfigurationProvider configuration,
            IMessageModelMapper mapper,
            IDeliveryService deliveryService, 
            IBlaiseService blaiseService)
        {
            _logger = logger;
            _configuration = configuration;
            _mapper = mapper;
            _deliveryService = deliveryService;
            _blaiseService = blaiseService;
        }

        public bool HandleMessage(string message)
        {
            try
            {
                //map the message taken off the queue to a model we can use
                var messageModel = _mapper.MapToMessageModel(message);

                if (string.IsNullOrWhiteSpace(messageModel.ServerParkName))
                {
                    _logger.Warn("A server park needs to be specified in order to deliver surveys");
                    return true;
                }

                if (!_blaiseService.ServerParkExists(messageModel.ServerParkName))
                {
                    _logger.Error($"The server park '{messageModel.ServerParkName}' does not exist on '{_configuration.VmName}'");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(messageModel.InstrumentName))
                {
                    return DeliverSingleInstrument(messageModel.ServerParkName, messageModel.InstrumentName);
                }
                
                _logger.Info($"No instrument name has been provided, all instruments on server park '{messageModel.ServerParkName}' will be delivered");

                return DeliverAllInstruments(messageModel.ServerParkName);
            }
            catch (Exception ex)
            {
                _logger.Error($"An exception occurred in processing message {message} - {ex.Message}");

                return false;
            }
        }

        private bool DeliverSingleInstrument(string serverParkName, string instrumentName)
        {
            if (!_blaiseService.InstrumentExists(serverParkName, instrumentName))
            {
                _logger.Error($"The instrument '{instrumentName}' does not exist on server park '{serverParkName}'");
                return false;
            }

            DeliverInstrument(serverParkName, instrumentName);

            return true;
        }

        private bool DeliverAllInstruments(string serverParkName)
        {
            var instrumentsInstalled = _blaiseService.GetInstrumentsInstalled(serverParkName).ToList();

            if (!instrumentsInstalled.Any())
            {
                _logger.Error($"The server park '{serverParkName}' does not have any instruments installed");
                return false;
            }

            foreach (var instrument in instrumentsInstalled)
            {
                DeliverInstrument(serverParkName, instrument);
            }

            return true;
        }

        private void DeliverInstrument(string serverParkName, string instrumentName)
        {
            var deliveryFile = _blaiseService.CreateDeliveryFile(serverParkName, instrumentName, _configuration.LocalProcessFolder);
            _deliveryService.UploadInstrumentFileToBucket(deliveryFile, instrumentName, _configuration.BucketName);
        }
    }
}
