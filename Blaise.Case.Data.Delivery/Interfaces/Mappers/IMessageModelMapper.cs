using Blaise.Case.Data.Delivery.Models;

namespace Blaise.Case.Data.Delivery.Interfaces.Mappers
{
    public interface IMessageModelMapper
    {
        MessageModel MapToMessageModel(string message);
    }
}
