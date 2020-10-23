
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Services;
using log4net;

namespace BlaiseDataDelivery.Services
{
    public class InitialiseService : IInitialiseService
    {
        private readonly IQueueService _queueService;
        private readonly IMessageHandler _messageHandler;

        public InitialiseService(
            IQueueService queueService, 
            IMessageHandler messageHandler)
        {
            _queueService = queueService;
            _messageHandler = messageHandler;
        }

        public void Start()
        {
            _queueService.Subscribe(_messageHandler);
        }

        public void Stop()
        {
            _queueService.CancelAllSubscriptions();
        }
    }
}
