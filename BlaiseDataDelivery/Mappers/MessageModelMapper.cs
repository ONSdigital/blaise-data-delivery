using BlaiseDataDelivery.Helpers;
using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Models;
using Newtonsoft.Json;

namespace BlaiseDataDelivery.Mappers
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
