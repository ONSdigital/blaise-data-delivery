using System;
using Blaise.Data.Delivery.Interfaces.Mappers;
using Blaise.Data.Delivery.Interfaces.Providers;
using Blaise.Data.Delivery.Interfaces.Services;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;

namespace Blaise.Data.Delivery.MessageHandlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configuration;
        private readonly IMessageModelMapper _mapper;
        private readonly IDeliveryService _deliveryService;
        private readonly IBlaiseApiService _blaiseService;

        public MessageHandler(
            ILog logger,
            IConfigurationProvider configuration,
            IMessageModelMapper mapper,
            IDeliveryService deliveryService,
            IBlaiseApiService blaiseService)
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
                    _logger.Info($"Instrument name '{messageModel.InstrumentName}' on server park '{messageModel.ServerParkName}' will be delivered");

                    return _deliveryService.DeliverSingleInstrument(messageModel.ServerParkName, messageModel.InstrumentName,
                        _configuration.LocalProcessFolder, _configuration.BucketName);
                }

                _logger.Info($"No instrument name has been provided, all instruments on server park '{messageModel.ServerParkName}' will be delivered");

                return _deliveryService.DeliverAllInstruments(messageModel.ServerParkName,
                    _configuration.LocalProcessFolder, _configuration.BucketName);
            }
            catch (Exception ex)
            {
                _logger.Error($"An exception occurred in processing message {message} - {ex.Message}");
                return false;
            }
        }
    }
}
