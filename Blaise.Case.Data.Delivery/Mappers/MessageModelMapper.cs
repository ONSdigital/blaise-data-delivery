using Blaise.Case.Data.Delivery.Helpers;
using Blaise.Case.Data.Delivery.Interfaces.Mappers;
using Blaise.Case.Data.Delivery.Models;
using Newtonsoft.Json;

namespace Blaise.Case.Data.Delivery.Mappers
{
    public class MessageModelMapper : IMessageModelMapper
    {
        public MessageModel MapToMessageModel(string message)
        {
            message.ThrowExceptionIfNullOrEmpty("message");

            return JsonConvert.DeserializeObject<MessageModel>(message);
        }
    }
}
