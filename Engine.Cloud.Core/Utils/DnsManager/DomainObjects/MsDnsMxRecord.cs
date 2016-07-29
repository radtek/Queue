using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsMxRecord : MsDnsRecord
    {
        public MsDnsMxRecord(string name, string value, MsZone msZone, int ttl, int priority)
            : base(name, value, msZone, ttl)
        {
            Priority = priority;
        }

        public int Priority { get; set; }

        public static MsDnsMxRecord Parse(ManagementObject record, MsZone msZone)
        {
            string data = (string)record.Properties["RecordData"].Value;
            string[] dataSplit = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int priority = int.Parse(dataSplit[0]);
            string value = dataSplit[1];

            MsDnsMxRecord dnsRecord = new MsDnsMxRecord((string)record.Properties["OwnerName"].Value,
                                                        value,
                                                        msZone,
                                                        (int)(UInt32)record.Properties["TTL"].Value,
                                                        priority);
            return dnsRecord;
        }
    }
}
