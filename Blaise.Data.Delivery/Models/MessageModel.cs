
using Newtonsoft.Json;

namespace Blaise.Data.Delivery.Models
{
    public class MessageModel
    {
        [JsonProperty("instrument")]
        public string InstrumentName { get; set; }

        [JsonProperty("serverpark")]
        public string ServerParkName { get; set; }
    }
}
