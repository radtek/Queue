using System.Collections.Generic;

namespace Engine.Cloud.Core.Model
{
    public class Resources
    {
        public int Vcpu { get; set; }

        public double Frequency { get; set; }
        public string FrequencyNominal { get; set; }
        public long Memory { get; set; }
        
        public List<NetworkInterface> NetworkInterfaces { get; set; }

        public Resources()
        {
            NetworkInterfaces = new List<NetworkInterface>();
        
        }
    }
}
