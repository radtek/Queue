using Newtonsoft.Json;

namespace Engine.Cloud.Core.Model
{
    [JsonObject]
    public class ClientBasicInfo
    {
        public string CustomerCode { get; set; }
        public long ClientId { get; set; }
        public string CpfCnpj { get; set; }
    }
}
