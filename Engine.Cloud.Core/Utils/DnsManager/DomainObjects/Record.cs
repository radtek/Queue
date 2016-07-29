using System;

namespace Engine.Cloud.Core.Utils.DnsManager.DomainObjects
{
    public class Record
    {
        public string Host { get; set; }
        public string Target { get; set; }
        public string TextRepresentation { get; set; }
        public TimeSpan Ttl { get; set; }
    }
}
