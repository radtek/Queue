using System;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsSrvRecord : MsDnsRecord
    {
        public string Service { get; set; }
        public string Protocol { get; set; }
        public int Port { get; set; }
        public int Priority { get; set; }
        public int Weight { get; set; }

        public MsDnsSrvRecord(string name, string value, MsZone msZone, int ttl, string service, string protocol, int port, int priority, int weight)
            : base(name, value, msZone, ttl)
        {
            Service = service;
            Protocol = protocol;
            Port = port;
            Priority = priority;
            Weight = weight;
        }

        public static MsDnsSrvRecord Parse(ManagementObject record, MsZone msZone)
        {
            string data = (string)record.Properties["RecordData"].Value;
            string[] dataSplit = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string service = "";
            string protocol = "";
            int port = int.Parse(dataSplit[2]);
            int priority = int.Parse(dataSplit[0]);
            int weight = int.Parse(dataSplit[1]);
            string value = dataSplit[3];

            var dnsRecord = new MsDnsSrvRecord((string)record.Properties["OwnerName"].Value,
                                                value,
                                                msZone,
                                                (int)(UInt32)record.Properties["TTL"].Value,
                                                service,
                                                protocol,
                                                port,
                                                priority,
                                                weight);
            return dnsRecord;
        }
    }
}
