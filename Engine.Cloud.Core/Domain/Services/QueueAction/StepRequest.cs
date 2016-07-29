using System.Collections.Generic;
using Utils;

namespace Engine.Cloud.Core.Domain.Services.QueueAction
{
    public class StepRequest : SerializableJsonObject<StepRequest>
    {
        public StepRequest()
        {
            Params = new Dictionary<string, string>();
        }
        public string Name { get; set; }
        public int TypeActionStepId { get; set; }
        public Dictionary<string, string> Params { get; set; }
    }

    public class SubActionResult : SerializableJsonObject<SubActionResult>
    {
        public string Result { get; set; }
    }
}
