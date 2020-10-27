
using System;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services;
using log4net;

namespace BlaiseDataDelivery.Services
{
    public class InitialiseService : IInitialiseService
    {
        private readonly ILog _logger;
        private readonly IQueueService _queueService;
        private readonly IMessageHandler _messageHandler;
        private readonly IConfigurationProvider _configurationProvider;

        public InitialiseService(
            ILog logger,
            IQueueService queueService,
            IMessageHandler messageHandler,
            IConfigurationProvider configurationProvider)
        {
            _logger = logger;
            _queueService = queueService;
            _messageHandler = messageHandler;
            _configurationProvider = configurationProvider;
        }

        public void Start()
        {
            _logger.Info($"Starting Data Delivery service on '{_configurationProvider.VmName}'");

            try
            {
                _queueService.Subscribe(_messageHandler);
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
                _queueService.CancelAllSubscriptions();
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
