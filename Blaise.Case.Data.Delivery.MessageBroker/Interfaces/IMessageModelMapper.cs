using Blaise.Case.Data.Delivery.MessageBroker.Models;

namespace Blaise.Case.Data.Delivery.MessageBroker.Interfaces
{
    public interface IMessageModelMapper
    {
        MessageModel MapToMessageModel(string message);
    }
}
