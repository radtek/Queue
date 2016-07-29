using System.Collections.Generic;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class MsDnsRecords
    {
        public IList<MsDnsARecord> MsDnsARecords { get; private set; }
        public IList<MsDnsCnameRecord> MsDnsCnameRecords { get; private set; }
        public IList<MsDnsMxRecord> MsDnsMxRecords { get; private set; }
        public IList<MsDnsSrvRecord> MsDnsSrvRecords { get; private set; }
        public IList<MsDnsNsRecord> MsDnsNsRecords { get; private set; }
        public IList<MsDnsTxtRecord> MsDnsTxtRecords { get; private set; }


        public MsDnsRecords()
        {
            MsDnsARecords = new List<MsDnsARecord>();
            MsDnsCnameRecords = new List<MsDnsCnameRecord>();
            MsDnsMxRecords = new List<MsDnsMxRecord>();
            MsDnsSrvRecords = new List<MsDnsSrvRecord>();
            MsDnsNsRecords = new List<MsDnsNsRecord>();
            MsDnsTxtRecords = new List<MsDnsTxtRecord>();

        }

        public void Parse(ManagementObject record, MsZone msZone)
        {
            if (GetRecordType(record) == "A")
            {
                MsDnsARecords.Add(MsDnsARecord.Parse(record, msZone));
            }
            if (GetRecordType(record) == "CNAME")
            {
                MsDnsCnameRecords.Add(MsDnsCnameRecord.Parse(record, msZone));
            }
            if (GetRecordType(record) == "MX")
            {
                MsDnsMxRecords.Add(MsDnsMxRecord.Parse(record, msZone));
            }
            if (GetRecordType(record) == "SRV")
            {
                MsDnsSrvRecords.Add(MsDnsSrvRecord.Parse(record, msZone));
            }
            if (GetRecordType(record) == "TXT")
            {
                MsDnsTxtRecords.Add(MsDnsTxtRecord.Parse(record, msZone));
            }
            if (GetRecordType(record) == "NS")
            {
                MsDnsNsRecords.Add(MsDnsNsRecord.Parse(record, msZone));
            }
        }

        private static string GetRecordType(ManagementBaseObject wmiObject)
        {
            string recordType = string.Empty;
            string[] recordParts = wmiObject["TextRepresentation"].ToString().Split(' ', '\t');
            if (recordParts.Length > 2)
            {
                //the third offset is the location in the textual version of the data where the record type is.
                //counting from zero that is location 2 in the array.
                recordType = recordParts[2];
            }

            return recordType;
        }
    }
}
