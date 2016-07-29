using System.Collections.Generic;

namespace Engine.Cloud.Core.Model.Results
{
    public class QueueActionResult
    {
        public QueueActionResult()
        {
            Results = new Dictionary<string, string>();
        }

        public StatusQueueAction Status { get; set; }
        public string Result { get; set; }

        public Dictionary<string, string> Results { get; set; }
    }
}
