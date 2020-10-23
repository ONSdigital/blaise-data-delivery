using System;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services;
using log4net;

namespace BlaiseDataDelivery.Services
{
    public class QueueService : IQueueService
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFluentQueueApi _queueApi;
        private readonly string _subscriptionId;

        public QueueService(
            ILog logger,
            IConfigurationProvider configurationProvider,
            IFluentQueueApi queueApi)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _queueApi = queueApi;

            _subscriptionId = $"{_configurationProvider.SubscriptionId}-{_configurationProvider.VmName}";
        }

        public void Subscribe(IMessageHandler messageHandler)
        {

            _queueApi
                .WithProject(_configurationProvider.ProjectId)
                .WithTopic(_configurationProvider.SubscriptionTopicId)
                .CreateSubscription(_subscriptionId)
                .WithExponentialBackOff(60)
                .WithDeadLetter(_configurationProvider.DeadletterTopicId)
                .StartConsuming(messageHandler, true);

            _logger.Info($"Subscription setup to '{_subscriptionId}' for project '{_configurationProvider.ProjectId}'");
        }

        public void CancelAllSubscriptions()
        {
            try
            {
                _queueApi
                    .StopConsuming();

            }
            catch (Exception e)
            {
                _logger.Error($"Could not stop consuming subscription because '{e.Message}'");
                throw;
            }


            _logger.Info($"Stopped consuming Subscription to '{_subscriptionId}' for project '{_configurationProvider.ProjectId}'");
        }
    }
}
