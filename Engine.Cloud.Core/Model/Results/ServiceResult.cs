using System.Collections.Generic;
using Utils;

namespace Engine.Cloud.Core.Model.Results
{
    public class ServiceResult : SerializableJsonObject<ServiceResult>
    {
        public ServiceResult()
        {
            Results = new Dictionary<string, string>();
        }

        public StatusService Status { get; set; }
        public string Result { get; set; }
        public Dictionary<string, string> Results { get; set; }
    }
}
