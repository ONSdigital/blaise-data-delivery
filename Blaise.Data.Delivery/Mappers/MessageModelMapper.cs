using Blaise.Data.Delivery.Helpers;
using Blaise.Data.Delivery.Interfaces.Mappers;
using Blaise.Data.Delivery.Models;
using Newtonsoft.Json;

namespace Blaise.Data.Delivery.Mappers
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
