using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Engine.Cloud.Core.Utils.DnsManager.DomainObjects;
using Engine.Cloud.Core.Utils.DnsManager.Enums;
using Engine.Cloud.Core.Utils.DnsManager.Factories;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.Core.Utils.DnsManager.Services
{
    public class DnsZoneService
    {
        private readonly ILogger _logger;
        private readonly int _key;
        private readonly IList<ManagementScope> _managementScopes;

        public DnsZoneService()
        {
            _logger = LogFactory.GetInstance();

            //_managementScopes = new List<ManagementScope>
            //                        {
            //                            ManagementScopeFactory.Create(new ConfigurationBuilder("DNS1")),
            //                            ManagementScopeFactory.Create(new ConfigurationBuilder("DNS2")),
            //                            ManagementScopeFactory.Create(new ConfigurationBuilder("DNS3")),
            //                            ManagementScopeFactory.Create(new ConfigurationBuilder("DNS4"))
            //                        };

            _managementScopes = new List<ManagementScope>();

            var dns1 = Task.Factory.StartNew(() => ManagementScopeFactory.Create(new ConfigurationBuilder("DNS1")));
            var dns2 = Task.Factory.StartNew(() => ManagementScopeFactory.Create(new ConfigurationBuilder("DNS2")));
            var dns3 = Task.Factory.StartNew(() => ManagementScopeFactory.Create(new ConfigurationBuilder("DNS3")));
            var dns4 = Task.Factory.StartNew(() => ManagementScopeFactory.Create(new ConfigurationBuilder("DNS4")));

            Task.WaitAll(dns1, dns2, dns3, dns4);

            _managementScopes.Add(dns1.Result);
            _managementScopes.Add(dns2.Result);
            _managementScopes.Add(dns3.Result);
            _managementScopes.Add(dns4.Result);
        }

        public IList<ManagementScope> GetManagementScope()
        {
            return _managementScopes;
        }

        public MsZone[] GetListOfZones()
        {
            ManagementClass mc = new ManagementClass(_managementScopes[0], new ManagementPath("MicrosoftDNS_Zone"), null);
            mc.Get();

            ManagementObjectCollection collection;
            List<MsZone> domains = new List<MsZone>();

            using (collection = mc.GetInstances())
            {
                var queryMo = from ManagementObject mo in collection select mo;
                Parallel.ForEach(queryMo, a => domains.Add(CreateObjectZone(a)));
            }

            return domains.ToArray();
        }


        public MsZone GetZone(string zoneName)
        {
            string result = string.Empty;
            try
            {
                Process scriptProc = new Process();
                scriptProc.StartInfo.UseShellExecute = false;
                scriptProc.StartInfo.RedirectStandardOutput = true;
                scriptProc.StartInfo.CreateNoWindow = true;
                scriptProc.StartInfo.FileName = @"cscript ";

                scriptProc.StartInfo.Arguments = AppSettings.GetString("DnsManager.Server.Path") + @" List " + zoneName + " /S " + AppSettings.GetString("DnsManager.Server.Dns1.IP") + " /U " + AppSettings.GetString("DnsManager.Server.Dns1.User") + " /W " + AppSettings.GetString("DnsManager.Server.Dns1.Pass");
                scriptProc.Start();
                result = scriptProc.StandardOutput.ReadToEnd();
                scriptProc.Close();
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1} - {2}", zoneName, LogUtils.GetCurrentMethod(this), result), exception);
                throw;
            }
            
            if (result.ToLower().Contains(zoneName))
            {
                return new MsZone() { Name = zoneName, ReverseZone = false, ZoneType = ZoneType.Primary };
            }
            return new MsZone();
        }


        public void NewZone(string zoneName)
        {
            CreatePrimaryDns(zoneName, _managementScopes[0]); // [0] Primary
            CreateSecondaryDns(zoneName, _managementScopes[1]); // [1] Secondary
            CreateTertiaryDns(zoneName, _managementScopes[2]); // [2] Tertiary
            CreateQuaternaryDns(zoneName, _managementScopes[3]); // [3] Quaternary
        }

        public void DeleteZone(string zoneName)
        {
            DeleteZone(zoneName, 1);
            DeleteZone(zoneName, 2);
            DeleteZone(zoneName, 3);
            DeleteZone(zoneName, 4);
        }

        private void DeleteZone(string zoneName, int dns)
        {

            string result = string.Empty;
            try
            {
                ReloadZone(zoneName, dns);

                Process scriptProc = new Process();
                scriptProc.StartInfo.UseShellExecute = false;
                scriptProc.StartInfo.RedirectStandardOutput = true;
                scriptProc.StartInfo.CreateNoWindow = true;
                scriptProc.StartInfo.FileName = @"cscript ";

                scriptProc.StartInfo.Arguments = AppSettings.GetString("DnsManager.Server.Path") + @" Delete " + zoneName + " /S " + AppSettings.GetString(String.Format("DnsManager.Server.Dns{0}.IP", dns)) + " /U " + AppSettings.GetString(String.Format("DnsManager.Server.Dns{0}.User", dns)) + " /W " + AppSettings.GetString(String.Format("DnsManager.Server.Dns{0}.Pass", dns));
                scriptProc.Start();
                result = scriptProc.StandardOutput.ReadToEnd();

                scriptProc.Close();
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1} - {2}", zoneName, LogUtils.GetCurrentMethod(this), result), exception);
                throw;
            }
        }


        private void ReloadZone(string zoneName, int dns)
        {
            try
            {
                Process scriptProc = new Process();
                scriptProc.StartInfo.UseShellExecute = false;
                scriptProc.StartInfo.RedirectStandardOutput = true;
                scriptProc.StartInfo.CreateNoWindow = true;
                scriptProc.StartInfo.FileName = @"cscript ";

                scriptProc.StartInfo.Arguments = AppSettings.GetString("DnsManager.Server.Path") + @" Reload " + zoneName + " /S " + AppSettings.GetString(String.Format("DnsManager.Server.Dns{0}.IP", dns)) + " /U " + AppSettings.GetString(String.Format("DnsManager.Server.Dns{0}.User", dns)) + " /W " + AppSettings.GetString(String.Format("DnsManager.Server.Dns{0}.Pass", dns));
                scriptProc.Start();
                scriptProc.Close();
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1}", zoneName, LogUtils.GetCurrentMethod(this)), exception);
                throw;
            }
        }

        private void CreatePrimaryDns(string zoneName, ManagementScope managementScope)
        {
            try
            {
                ManagementObject mc = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_Zone"), null);
                mc.Get();
                ManagementBaseObject parameters = mc.GetMethodParameters("CreateZone");

                /*
                [in]            string ZoneName,
                [in]            uint32 ZoneType,
                [in]            boolean DsIntegrated,   (will always be false for us, if you need AD integration you will need to change this.
                [in, optional]  string DataFileName,
                [in, optional]  string IpAddr[],
                [in, optional]  string AdminEmailName,
                */

                parameters["ZoneName"] = zoneName;
                parameters["DsIntegrated"] = 0; //false
                parameters["ZoneType"] = (UInt32)ZoneType.Primary;
                mc.InvokeMethod("CreateZone", parameters, null);

                ManagementObject mc2 = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_NSType"), null);
                ManagementBaseObject p = mc2.GetMethodParameters("CreateInstanceFromPropertyData");
                p.Properties["DnsServerName"].Value = managementScope.Path.Server;
                //p.Properties["TTL"].Value = record.TTL;
                p.Properties["ContainerName"].Value = zoneName;
                p.Properties["OwnerName"].Value = zoneName;
                var nameServerSecondary = new ConfigurationBuilder("DNS2").Ns;
                p.Properties["NSHost"].Value = nameServerSecondary;
                mc2.InvokeMethod("CreateInstanceFromPropertyData", p, null);

                ManagementObject mc3 = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_NSType"), null);
                ManagementBaseObject p3 = mc3.GetMethodParameters("CreateInstanceFromPropertyData");
                p3.Properties["DnsServerName"].Value = managementScope.Path.Server;
                p3.Properties["ContainerName"].Value = zoneName;
                p3.Properties["OwnerName"].Value = zoneName;
                var nameServerTertiary = new ConfigurationBuilder("DNS3").Ns;
                p3.Properties["NSHost"].Value = nameServerTertiary;
                mc3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);

                ManagementObject mc4 = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_NSType"), null);
                ManagementBaseObject p4 = mc4.GetMethodParameters("CreateInstanceFromPropertyData");
                p4.Properties["DnsServerName"].Value = managementScope.Path.Server;
                p4.Properties["ContainerName"].Value = zoneName;
                p4.Properties["OwnerName"].Value = zoneName;
                var nameServerQuaternary = new ConfigurationBuilder("DNS4").Ns;
                p4.Properties["NSHost"].Value = nameServerQuaternary;
                mc4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1}", zoneName, LogUtils.GetCurrentMethod(this)), exception);
                throw;
            }
        }


        private void CreateSecondaryDns(string zoneName, ManagementScope managementScope)
        {
            try
            {
                ManagementObject mc = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_Zone"), null);
                mc.Get();
                ManagementBaseObject parameters = mc.GetMethodParameters("CreateZone");

                /*
                [in]            string ZoneName,
                [in]            uint32 ZoneType,
                [in]            boolean DsIntegrated,   (will always be false for us, if you need AD integration you will need to change this.
                [in, optional]  string DataFileName,
                [in, optional]  string IpAddr[],
                [in, optional]  string AdminEmailName,
                */

                parameters["ZoneName"] = zoneName;
                parameters["DsIntegrated"] = 0; //false

                parameters["ZoneType"] = (UInt32)ZoneType.Secondary;
                string[] masterServer = { new ConfigurationBuilder("DNS1").IP };
                parameters["IpAddr"] = masterServer;

                mc.InvokeMethod("CreateZone", parameters, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1}", zoneName, LogUtils.GetCurrentMethod(this)), exception);
                throw;
            }
        }

        private void CreateTertiaryDns(string zoneName, ManagementScope managementScope)
        {
            try
            {
                ManagementObject mc = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_Zone"), null);
                mc.Get();
                ManagementBaseObject parameters = mc.GetMethodParameters("CreateZone");

                /*
                [in]            string ZoneName,
                [in]            uint32 ZoneType,
                [in]            boolean DsIntegrated,   (will always be false for us, if you need AD integration you will need to change this.
                [in, optional]  string DataFileName,
                [in, optional]  string IpAddr[],
                [in, optional]  string AdminEmailName,
                */

                parameters["ZoneName"] = zoneName;
                parameters["DsIntegrated"] = 0; //false

                parameters["ZoneType"] = (UInt32)ZoneType.Secondary;
                string[] masterServer = { new ConfigurationBuilder("DNS1").IP };
                parameters["IpAddr"] = masterServer;

                mc.InvokeMethod("CreateZone", parameters, null);

            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1}", zoneName, LogUtils.GetCurrentMethod(this)), exception);
                throw;
            }
        }

        private void CreateQuaternaryDns(string zoneName, ManagementScope managementScope)
        {
            try
            {
                ManagementObject mc = new ManagementClass(managementScope, new ManagementPath("MicrosoftDNS_Zone"), null);
                mc.Get();
                ManagementBaseObject parameters = mc.GetMethodParameters("CreateZone");

                /*
                [in]            string ZoneName,
                [in]            uint32 ZoneType,
                [in]            boolean DsIntegrated,   (will always be false for us, if you need AD integration you will need to change this.
                [in, optional]  string DataFileName,
                [in, optional]  string IpAddr[],
                [in, optional]  string AdminEmailName,
                */

                parameters["ZoneName"] = zoneName;
                parameters["DsIntegrated"] = 0; //false

                parameters["ZoneType"] = (UInt32)ZoneType.Secondary;
                string[] masterServer = { new ConfigurationBuilder("DNS1").IP };
                parameters["IpAddr"] = masterServer;

                mc.InvokeMethod("CreateZone", parameters, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("Gerenciador de DNS - {0} - {1}", zoneName, LogUtils.GetCurrentMethod(this)), exception);
                throw;
            }
        }

        private static MsZone CreateObjectZone(ManagementObject createdEntry)
        {
            return new MsZone
            {
                Name = createdEntry["ContainerName"].ToString(),
                ZoneType = GetZoneType(createdEntry),
                ReverseZone = IsReverseZone(createdEntry)
            };
        }


        private static bool IsReverseZone(ManagementBaseObject createdEntry)
        {
            return createdEntry["Reverse"].ToString() == "1";
        }


        private static ZoneType GetZoneType(ManagementBaseObject createdEntry)
        {
            return (ZoneType)Convert.ToInt32(createdEntry["ZoneType"]);
        }
    }
}
