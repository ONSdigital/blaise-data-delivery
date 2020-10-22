
using Newtonsoft.Json;

namespace BlaiseDataDelivery.Models
{
    public class MessageModel
    {
        [JsonProperty("source_instrument")]
        public string InstrumentName { get; set; }

        [JsonProperty("source_serverpark")]
        public string ServerParkName { get; set; }
    }
}
