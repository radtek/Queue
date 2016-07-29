using Newtonsoft.Json;

namespace Engine.Cloud.Core.Utils.VMMService.DataContract
{
    [JsonObject]
    public class VirtualmachineNetworkContract
    {
        [JsonProperty("MACAddress")]
        public string MacAddress { get; set; }

        [JsonProperty("ipv4")]
        public string Ipv4 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("bandwidth")]
        public string Bandwidth { get; set; }

        [JsonProperty("vlanid")]
        public string VlanId { get; set; }
    }
}
