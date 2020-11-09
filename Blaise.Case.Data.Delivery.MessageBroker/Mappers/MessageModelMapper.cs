using Blaise.Case.Data.Delivery.Core.Extensions;
using Blaise.Case.Data.Delivery.MessageBroker.Interfaces;
using Blaise.Case.Data.Delivery.MessageBroker.Models;
using Newtonsoft.Json;

namespace Blaise.Case.Data.Delivery.MessageBroker.Mappers
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
