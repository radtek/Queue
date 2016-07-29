using System;
using System.Configuration;

namespace Engine.Cloud.Core.Utils.DnsManager.Factories
{
    public class ConfigurationBuilder
    {
        public string Ns;
        public string IP { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }


        public ConfigurationBuilder(string key)
        {
            IP = ConfigurationManager.AppSettings[String.Format("DnsManager.Server.{0}.IP", key)];
            User = ConfigurationManager.AppSettings[String.Format("DnsManager.Server.{0}.User", key)];
            Pass = ConfigurationManager.AppSettings[String.Format("DnsManager.Server.{0}.Pass", key)];
            Ns = ConfigurationManager.AppSettings[String.Format("DnsManager.Server.{0}.Ns", key)];
        }
    }
}
