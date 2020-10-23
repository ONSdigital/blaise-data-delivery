
using Newtonsoft.Json;

namespace BlaiseDataDelivery.Models
{
    public class MessageModel
    {
        [JsonProperty("instrument")]
        public string InstrumentName { get; set; }

        [JsonProperty("serverpark")]
        public string ServerParkName { get; set; }
    }
}
