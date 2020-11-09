

using Blaise.Nuget.PubSub.Contracts.Interfaces;

namespace Blaise.Case.Data.Delivery.MessageBroker.Interfaces
{
    public interface IMessageBrokerService 
    {
        void Subscribe(IMessageHandler messageHandler);

        void CancelAllSubscriptions();
    }
}
