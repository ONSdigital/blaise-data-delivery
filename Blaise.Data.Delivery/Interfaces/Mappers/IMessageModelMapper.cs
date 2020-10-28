using Blaise.Data.Delivery.Models;

namespace Blaise.Data.Delivery.Interfaces.Mappers
{
    public interface IMessageModelMapper
    {
        MessageModel MapToMessageModel(string message);
    }
}
