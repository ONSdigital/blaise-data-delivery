using System;
using Blaise.Case.Data.Delivery.Core.Interfaces;
using Blaise.Case.Data.Delivery.MessageBroker.Interfaces;
using Blaise.Case.Data.Delivery.WindowsService.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;

namespace Blaise.Case.Data.Delivery.WindowsService
{
    public class InitialiseWindowsService : IInitialiseWindowsService
    {
        private readonly ILog _logger;
        private readonly IMessageBrokerService _messageBrokerService;
        private readonly IMessageHandler _messageHandler;
        private readonly IConfigurationProvider _configurationProvider;

        public InitialiseWindowsService(
            ILog logger,
            IMessageBrokerService messageBrokerService,
            IMessageHandler messageHandler,
            IConfigurationProvider configurationProvider)
        {
            _logger = logger;
            _messageBrokerService = messageBrokerService;
            _messageHandler = messageHandler;
            _configurationProvider = configurationProvider;
        }

        public void Start()
        {
            _logger.Info($"Starting Data Delivery service on '{_configurationProvider.VmName}'");

            try
            {
                _messageBrokerService.Subscribe(_messageHandler);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn($"There was an error starting the Data Delivery service");
                throw;
            }

            _logger.Info($"Data Delivery service started on '{_configurationProvider.VmName}'");
        }

        public void Stop()
        {
            _logger.Info($"Stopping Data Delivery service on '{_configurationProvider.VmName}'");

            try
            {
                _messageBrokerService.CancelAllSubscriptions();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn($"There was an error stopping the Data Delivery service");
                throw;
            }
           
            _logger.Info($"Data Delivery service stopped on '{_configurationProvider.VmName}'");
        }
    }
}
