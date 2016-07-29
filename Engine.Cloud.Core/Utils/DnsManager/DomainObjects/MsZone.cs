using Engine.Cloud.Core.Utils.DnsManager.Enums;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsZone
    {
        public string Name { get; set; }
        public ZoneType ZoneType { get; set; }
        public bool ReverseZone { get; set; }
    }
}
