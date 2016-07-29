using System;
using Utils;

namespace Engine.Cloud.Core.Model
{
    public partial class ImageInfos : SerializableJsonObject<ImageInfos>
    {
        public int ImageId { get; set; }
        public string Name { get; set; }
        public Nullable<bool> Active { get; set; }
        public int TypeHipervisorId { get; set; }
        public string Password { get; set; }
        public string Plataform { get; set; }
	    public string Distribution { get; set; }
        public string Version { get; set; }
        public string Architecture { get; set; }
        public string TypeInstall { get; set; }
        public string Observation { get; set; }
        public Nullable<int> DiskSO { get; set; }
        public Nullable<bool> Internal { get; set; }
    }
}
