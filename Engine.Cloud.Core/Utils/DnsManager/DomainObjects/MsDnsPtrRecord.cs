
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsPtrRecord : MsDnsRecord
    {

        public MsDnsPtrRecord(string name, string value, MsZone msZone, int ttl)
            : base(name, value, msZone, ttl)
        {

        }

        public MsDnsPtrRecord(string name, MsZone msZone, int ttl)
            : base(name, msZone, ttl)
        {

        }

        public static MsDnsPtrRecord Parse(ManagementObject record)
        {
            MsDnsPtrRecord dnsRecord = new MsDnsPtrRecord(
                                                        (string)record.Properties["OwnerName"].ToString().Split('.')[0],
                                                        (string)record.Properties["RecordData"].Value,
                                                        new MsZone { Name = (string)record.Properties["ContainerName"].Value },
                                                        0);
            return dnsRecord;
        }
    }
}
