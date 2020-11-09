using Blaise.Case.Data.Delivery.Core.Interfaces;
using Blaise.Case.Data.Delivery.MessageBroker.Interfaces;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;

namespace Blaise.Case.Data.Delivery.MessageBroker
{
    public class MessageBrokerService : IMessageBrokerService
    {
        private readonly ILog _logger;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IFluentQueueApi _queueApi;

        public MessageBrokerService(
            ILog logger,
            IConfigurationProvider configurationProvider,
            IFluentQueueApi queueApi)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
            _queueApi = queueApi;
        }

        public void Subscribe(IMessageHandler messageHandler)
        {

            _queueApi
                .WithProject(_configurationProvider.ProjectId)
                .WithSubscription(_configurationProvider.SubscriptionId)
                .WithExponentialBackOff(60)
                .WithDeadLetter(_configurationProvider.DeadletterTopicId)
                .StartConsuming(messageHandler, true);

            _logger.Info($"Subscription setup to '{_configurationProvider.SubscriptionId}' " +
                         $"for project '{_configurationProvider.ProjectId}'");
        }

        public void CancelAllSubscriptions()
        {
            _queueApi.StopConsuming();

            _logger.Info($"Stopped consuming Subscription to '{_configurationProvider.SubscriptionId}'" +
                         $" for project '{_configurationProvider.ProjectId}'");
        }
    }
}
