
using System;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsCnameRecord : MsDnsRecord
    {
        public MsDnsCnameRecord(string name, string value, MsZone msZone, int ttl)
            : base(name, value, msZone, ttl)
        {
        }

        public static MsDnsCnameRecord Parse(ManagementObject record, MsZone msZone)
        {
            MsDnsCnameRecord dnsRecord = new MsDnsCnameRecord((string)record.Properties["OwnerName"].Value,
                                                              (string)record.Properties["RecordData"].Value,
                                                              msZone,
                                                              (int)(UInt32)record.Properties["TTL"].Value);

            return dnsRecord;
        }
    }
}
