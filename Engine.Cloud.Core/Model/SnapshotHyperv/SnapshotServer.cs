using Newtonsoft.Json;

namespace Engine.Cloud.Core.Model.SnapshotHyperv
{
    [JsonObject]
    public class SnapshotServer
    {
        public string Name { get; set; }
        public string AddedTime { get; set; }
        public string IDCheckpoint { get; set; }
        public int IsCurrentCheckpoint { get; set; }
    }
}
