
using Newtonsoft.Json;

namespace Blaise.Case.Data.Delivery.MessageBroker.Models
{
    public class MessageModel
    {
        [JsonProperty("instrument")]
        public string InstrumentName { get; set; }

        [JsonProperty("serverpark")]
        public string ServerParkName { get; set; }
    }
}
