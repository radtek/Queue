using System;
using Engine.Cloud.Core.Utils.DnsManager.Enums;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public abstract class MsDnsRecord : MarshalByRefObject
    {
        private int ttl;

        /// <summary>
        /// Gets or sets the value part of the record. This would hold
        /// an IP address, or hostname for example.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the name part of the record. This represents the
        /// first part of the owner value of a record. For example, the owner
        /// is "fabrikham.contoso.com", where "fabrikham" is the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent zone part of the record. This represents
        /// the container of the record. For example, contoso.com.
        /// </summary>
        public MsZone MsZone { get; set; }

        /// <summary>
        /// Gets or sets the TTL (time to live) part of the record. This
        /// represents how long a record will live.
        /// </summary>
        public int TTL { get; set; }

        /// <summary>
        /// Gets the container part of the record. This is found as the Domain
        /// property of the Zone property in this object.
        /// </summary>
        public string ZoneName
        {
            get { return MsZone.Name; }
        }

        /// <summary>
        /// Gets the owner part of the record. This represents the full owner
        /// name of the record which is comprised of the Container and the Name.
        /// </summary>
        public string FullName
        {
            get
            {
                // For named records, concatenate with Container.
                if (!String.IsNullOrEmpty(Name))
                {
                    return String.Format("{0}.{1}", Name, ZoneName);
                }

                // Owner same as container.
                return ZoneName;
            }
        }

        /// <summary>
        /// Gets the enumerated type equivilent of the most derrived version of this object.
        /// </summary>
        public MsDnsRecordType DnsType
        {
            get
            {
                if (this is MsDnsARecord)
                {
                    return MsDnsRecordType.A;
                }
                if (this is MsDnsCnameRecord)
                {
                    return MsDnsRecordType.Cname;
                }
                if (this is MsDnsMxRecord)
                {
                    return MsDnsRecordType.Mx;
                }
                if (this is MsDnsSrvRecord)
                {
                    return MsDnsRecordType.Srv;
                }
                if (this is MsDnsNsRecord)
                {
                    return MsDnsRecordType.Ns;
                }
                if (this is MsDnsTxtRecord)
                {
                    return MsDnsRecordType.Txt;
                }
                if (this is MsDnsPtrRecord)
                {
                    return MsDnsRecordType.Ptr;
                }

                throw new NotSupportedException(
                    "Type " + GetType().FullName + " not supported.");
            }
        }

        /// <summary>
        /// Initialize a full DNS record with all properties set.
        /// </summary>
        /// <param name="name">First part of the "owner" property.</param>
        /// <param name="value">IP address, hostname, etc.</param>
        /// <param name="zone">Parent zone for the record.</param>
        /// <param name="msZone">Objects MsZone</param>
        /// <param name="ttl">Record time to live value.</param>
        public MsDnsRecord(string name, string value, MsZone msZone, int ttl)
        {
            Name = name == null ? "" : name.Replace(msZone.Name, null).Trim('.');
            Value = value;
            MsZone = msZone;
            TTL = ttl;
        }

        public MsDnsRecord(string name, DomainObjects.MsZone msZone, int ttl)
        {
            // TODO: Complete member initialization
            this.Name = name;
            this.MsZone = msZone;
            this.ttl = ttl;
        }
    }
}
