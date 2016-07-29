using System;
using System.Collections.Generic;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsARecord : MsDnsRecord
    {
        public MsDnsARecord(string name, string value, MsZone msZone, int ttl)
            : base(name, value, msZone, ttl)
        {
        }

        public static MsDnsARecord Parse(ManagementObject record, MsZone msZone)
        {
            MsDnsARecord dnsRecord = new MsDnsARecord((string)record.Properties["OwnerName"].Value,
                                                      (string)record.Properties["RecordData"].Value,
                                                      msZone,
                                                      (int)(UInt32)record.Properties["TTL"].Value);

            return dnsRecord;
        }
    }
}
