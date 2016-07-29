
using System;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsTxtRecord : MsDnsRecord
    {
        public MsDnsTxtRecord(string name, string value, MsZone msZone, int ttl)
            : base(name, value, msZone, ttl)
        {
        }

        public static MsDnsTxtRecord Parse(ManagementObject record, MsZone msZone)
        {
            MsDnsTxtRecord dnsRecord = new MsDnsTxtRecord((string)record.Properties["OwnerName"].Value,
                                                          (string)record.Properties["RecordData"].Value,
                                                          msZone,
                                                          (int)(UInt32)record.Properties["TTL"].Value);
            return dnsRecord;
        }
    }
}
