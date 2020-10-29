

using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Data.Delivery.Interfaces.Services
{
    public interface IQueueService
    {
        void Subscribe(IMessageHandler messageHandler);

        void CancelAllSubscriptions();
    }
}
