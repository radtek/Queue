using System.Collections.Generic;

namespace Engine.Cloud.Core.Model
{
    public class NetworkInterface
    {
        public string Name { get; set; }
        public decimal BandWidth { get; set; }
        public string Mac { get; set; }
        public List<Ip> Ips { get; set; }
     
        public string Vlan { get; set; }
        public string Type { get; set; }
        public NetworkInterface()
        {
            Ips = new List<Ip>();
     
        }
    }
}
