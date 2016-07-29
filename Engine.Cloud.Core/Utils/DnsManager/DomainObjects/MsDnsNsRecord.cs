
using System;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsNsRecord : MsDnsRecord
    {
        public MsDnsNsRecord(string name, string value, MsZone msZone, int ttl)
            : base(name, value, msZone, ttl)
        {
        }

        public static MsDnsNsRecord Parse(ManagementObject record, MsZone msZone)
        {
            MsDnsNsRecord dnsRecord = new MsDnsNsRecord((string)record.Properties["OwnerName"].Value,
                                          (string)record.Properties["RecordData"].Value,
                                          msZone,
                                          (int)(UInt32)record.Properties["TTL"].Value);

            return dnsRecord;
        }
    }
}
