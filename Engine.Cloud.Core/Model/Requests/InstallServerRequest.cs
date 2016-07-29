using Newtonsoft.Json;

namespace Engine.Cloud.Core.Model.Requests
{
    public class InstallServerRequest
    {
        public string CustomerCode { get; set; }
        public int ImageId { get; set; }
        public int TypeImageId { get; set; }
        public int PlanId { get; set; }
        public int Frequency { get; set; }
        public byte Vcpu { get; set; }
        public int Memory { get; set; }
        public int Disk { get; set; }
        public decimal[] BandWidths { get; set; }
        public int TypeManagementId { get; set; }
        public int Partitions { get; set; }
        public string FormatDisk { get; set; }
        public long ServerId { get; set; }

        public static decimal[] ParseBandwidths(string bandwidths)
        {
            if (string.IsNullOrEmpty(bandwidths))
            {
                return new decimal[] {};
            }
            var result = JsonConvert.DeserializeObject<decimal[]>(bandwidths);
            return result;
        }
    }
}
