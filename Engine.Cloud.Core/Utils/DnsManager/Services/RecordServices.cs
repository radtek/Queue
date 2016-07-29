using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using Engine.Cloud.Core.Utils.DnsManager.DomainObjects;
using Engine.Cloud.Core.Utils.DnsManager.Factories;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.Core.Utils.DnsManager.Services
{
    public class RecordServices
    {
        private readonly ILogger _logger;
        private readonly IList<ManagementScope> _managementScopes;

        public RecordServices()
        {
            _logger = LogFactory.GetInstance();
            _managementScopes = new List<ManagementScope>
                                    {
                                        ManagementScopeFactory.Create(new ConfigurationBuilder("DNS1")),
                                        ManagementScopeFactory.Create(new ConfigurationBuilder("DNS2")),
                                        ManagementScopeFactory.Create(new ConfigurationBuilder("DNS3")),
                                        ManagementScopeFactory.Create(new ConfigurationBuilder("DNS4"))
                                    };

        }

        public RecordServices(IList<ManagementScope> managementScopes)
        {
            _managementScopes = managementScopes;
        }

        public RecordServices(string key)
        {
            _managementScopes = new List<ManagementScope>
                                    {
                                        ManagementScopeFactory.Create(new ConfigurationBuilder(key))
                                    };
        }

        public IList<ManagementScope> GetManagementScope()
        {
            return _managementScopes;
        }

        public void CreateRecord(MsDnsARecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_AType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.FullName;

                p1.Properties["IPAddress"].Value = record.Value;
                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_AType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.FullName;

                //p2.Properties["IPAddress"].Value = record.Value;
                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_AType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.FullName;

                //p3.Properties["IPAddress"].Value = record.Value;
                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_AType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.FullName;

                //p4.Properties["IPAddress"].Value = record.Value;
                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw;

            }

        }

        public void CreateRecord(MsDnsNsRecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_NSType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.FullName;

                p1.Properties["NSHost"].Value = record.Value;
                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_NSType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.FullName;

                //p2.Properties["NSHost"].Value = record.Value;
                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_NSType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.FullName;

                //p3.Properties["NSHost"].Value = record.Value;
                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_NSType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.FullName;

                //p4.Properties["NSHost"].Value = record.Value;
                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }

        public void CreateRecord(MsDnsCnameRecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_CNAMEType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.FullName;

                p1.Properties["PrimaryName"].Value = record.Value;
                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_CNAMEType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.FullName;

                //p2.Properties["PrimaryName"].Value = record.Value;
                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_CNAMEType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.FullName;

                //p3.Properties["PrimaryName"].Value = record.Value;
                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_CNAMEType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.FullName;

                //p4.Properties["PrimaryName"].Value = record.Value;
                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }

        public void CreateRecord(MsDnsMxRecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_MXType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.FullName;

                p1.Properties["Preference"].Value = record.Priority <= 0 ? 1 : record.Priority;
                p1.Properties["MailExchange"].Value = record.Value;
                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_MXType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.FullName;

                //p2.Properties["Preference"].Value = record.Priority <= 0 ? 2 : record.Priority;
                //p2.Properties["MailExchange"].Value = record.Value;
                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_MXType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.FullName;

                //p3.Properties["Preference"].Value = record.Priority <= 0 ? 3 : record.Priority;
                //p3.Properties["MailExchange"].Value = record.Value;
                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_MXType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.FullName;

                //p4.Properties["Preference"].Value = record.Priority <= 0 ? 4 : record.Priority;
                //p4.Properties["MailExchange"].Value = record.Value;
                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }

        public void CreateRecord(MsDnsSrvRecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_SRVType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.Service + "." + record.Protocol + "." + record.FullName;

                p1.Properties["Priority"].Value = record.Priority <= 0 ? 1 : record.Priority;
                p1.Properties["Weight"].Value = record.Weight <= 0 ? 1 : record.Weight;
                p1.Properties["Port"].Value = record.Port <= 0 ? 1 : record.Port;
                p1.Properties["SRVDomainName"].Value = record.Value;
                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_SRVType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.Service +"."+ record.Protocol +"."+ record.FullName;

                //p2.Properties["Priority"].Value = record.Priority <= 0 ? 2 : record.Priority;
                //p2.Properties["Weight"].Value = record.Weight <= 0 ? 2 : record.Weight;
                //p2.Properties["Port"].Value = record.Port <= 0 ? 2 : record.Port;
                //p2.Properties["SRVDomainName"].Value = record.Value;
                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_SRVType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.Service +"."+ record.Protocol +"."+ record.FullName;

                //p3.Properties["Priority"].Value = record.Priority <= 0 ? 3 : record.Priority;
                //p3.Properties["Weight"].Value = record.Weight <= 0 ? 3 : record.Weight;
                //p3.Properties["Port"].Value = record.Port <= 0 ? 3 : record.Port;
                //p3.Properties["SRVDomainName"].Value = record.Value;
                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_SRVType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.Service +"."+ record.Protocol +"."+ record.FullName;

                //p4.Properties["Priority"].Value = record.Priority <= 0 ? 4 : record.Priority;
                //p4.Properties["Weight"].Value = record.Weight <= 0 ? 4 : record.Weight;
                //p4.Properties["Port"].Value = record.Port <= 0 ? 4 : record.Port;
                //p4.Properties["SRVDomainName"].Value = record.Value;
                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }

        public void CreateRecord(MsDnsTxtRecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_TXTType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.FullName;

                p1.Properties["RecordClass"].Value = 1;
                p1.Properties["DescriptiveText"].Value = record.Value;

                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_TXTType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.FullName;

                //p2.Properties["RecordClass"].Value = 2;
                //p2.Properties["DescriptiveText"].Value = record.Value;

                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_TXTType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.FullName;

                //p3.Properties["RecordClass"].Value = 3;
                //p3.Properties["DescriptiveText"].Value = record.Value;

                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_TXTType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.FullName;

                //p4.Properties["RecordClass"].Value = 4;
                //p4.Properties["DescriptiveText"].Value = record.Value;

                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }

        public void CreateRecord(MsDnsPtrRecord record)
        {
            try
            {
                ManagementPath path1 = new ManagementPath("MicrosoftDNS_PTRType");
                ManagementClass zone1 = new ManagementClass(_managementScopes[0], path1, null);
                ManagementBaseObject p1 = zone1.GetMethodParameters("CreateInstanceFromPropertyData");

                p1.Properties["DnsServerName"].Value = _managementScopes[0].Path.Server;
                p1.Properties["ContainerName"].Value = record.ZoneName;
                p1.Properties["OwnerName"].Value = record.FullName;

                p1.Properties["RecordClass"].Value = 1;
                p1.Properties["PTRDomainName"].Value = record.Value;

                zone1.InvokeMethod("CreateInstanceFromPropertyData", p1, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path2 = new ManagementPath("MicrosoftDNS_PTRType");
                //ManagementClass zone2 = new ManagementClass(_managementScopes[1], path2, null);
                //ManagementBaseObject p2 = zone2.GetMethodParameters("CreateInstanceFromPropertyData");

                //p2.Properties["DnsServerName"].Value = _managementScopes[1].Path.Server;
                //p2.Properties["ContainerName"].Value = record.ZoneName;
                //p2.Properties["OwnerName"].Value = record.FullName;

                //p2.Properties["RecordClass"].Value = 2;
                //p2.Properties["PTRDomainName"].Value = record.Value;

                //zone2.InvokeMethod("CreateInstanceFromPropertyData", p2, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path3 = new ManagementPath("MicrosoftDNS_PTRType");
                //ManagementClass zone3 = new ManagementClass(_managementScopes[2], path3, null);
                //ManagementBaseObject p3 = zone3.GetMethodParameters("CreateInstanceFromPropertyData");

                //p3.Properties["DnsServerName"].Value = _managementScopes[2].Path.Server;
                //p3.Properties["ContainerName"].Value = record.ZoneName;
                //p3.Properties["OwnerName"].Value = record.FullName;

                //p3.Properties["RecordClass"].Value = 3;
                //p3.Properties["PTRDomainName"].Value = record.Value;

                //zone3.InvokeMethod("CreateInstanceFromPropertyData", p3, null);
                ////-------------------------------------------------------------------------------------
                //ManagementPath path4 = new ManagementPath("MicrosoftDNS_PTRType");
                //ManagementClass zone4 = new ManagementClass(_managementScopes[3], path4, null);
                //ManagementBaseObject p4 = zone4.GetMethodParameters("CreateInstanceFromPropertyData");

                //p4.Properties["DnsServerName"].Value = _managementScopes[3].Path.Server;
                //p4.Properties["ContainerName"].Value = record.ZoneName;
                //p4.Properties["OwnerName"].Value = record.FullName;

                //p4.Properties["RecordClass"].Value = 4;
                //p4.Properties["PTRDomainName"].Value = record.Value;

                //zone4.InvokeMethod("CreateInstanceFromPropertyData", p4, null);
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }

        public IList<MsDnsARecord> GetARecords(MsZone msZone)
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_AType WHERE ContainerName = '" + msZone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScopes[0], query);
            ManagementObjectCollection recordCollection;

            IList<MsDnsARecord> recordList = new List<MsDnsARecord>();

            using (recordCollection = searcher.Get())
            {
                var queryMo = from ManagementObject mo in recordCollection select mo;
                Parallel.ForEach(queryMo, record => recordList.Add(MsDnsARecord.Parse(record, msZone)));
            }

            return recordList;
        }

        public IList<MsDnsCnameRecord> GetCnameRecords(MsZone msZone)
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_CNAMEType WHERE ContainerName = '" + msZone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScopes[0], query);
            ManagementObjectCollection recordCollection;

            IList<MsDnsCnameRecord> recordList = new List<MsDnsCnameRecord>();
            using (recordCollection = searcher.Get())
            {
                var queryMo = from ManagementObject mo in recordCollection select mo;
                Parallel.ForEach(queryMo, record => recordList.Add(MsDnsCnameRecord.Parse(record, msZone)));
            }

            return recordList;
        }

        public IList<MsDnsMxRecord> GetMxRecords(MsZone msZone)
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_MXType WHERE ContainerName = '" + msZone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScopes[0], query);
            ManagementObjectCollection recordCollection;
            List<MsDnsMxRecord> recordList = new List<MsDnsMxRecord>();

            using (recordCollection = searcher.Get())
            {
                var queryMo = from ManagementObject mo in recordCollection select mo;
                Parallel.ForEach(queryMo, record => recordList.Add(MsDnsMxRecord.Parse(record, msZone)));
            }

            return recordList;
        }

        public IList<MsDnsTxtRecord> GetTxtRecords(MsZone msZone)
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_TXTType WHERE ContainerName = '" + msZone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScopes[0], query);
            ManagementObjectCollection recordCollection;

            List<MsDnsTxtRecord> recordList = new List<MsDnsTxtRecord>();
            using (recordCollection = searcher.Get())
            {
                var queryMo = from ManagementObject mo in recordCollection select mo;
                Parallel.ForEach(queryMo, record => recordList.Add(MsDnsTxtRecord.Parse(record, msZone)));
            }

            return recordList;
        }

        public MsDnsRecords GetRecords(MsZone msZone)
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_ResourceRecord WHERE ContainerName = '" + msZone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScopes[0], query);
            ManagementObjectCollection recordCollection = searcher.Get();

            MsDnsRecords msDnsRecords = new MsDnsRecords();

            try
            {
                foreach (ManagementObject record in recordCollection)
                {
                        msDnsRecords.Parse(record, msZone);
                }
            }
            catch (Exception ex)
            {
                return msDnsRecords;
            }

            return msDnsRecords;
        }

        public List<MsDnsPtrRecord> GetReverseRecord(string ipAddress)
        {
            List<MsDnsPtrRecord> records = new List<MsDnsPtrRecord>();

            try
            {
                string[] octetos = ipAddress.Split('.');
                string container = octetos[2] + "." + octetos[1] + "." + octetos[0] + ".in-addr.arpa";
                string ownerName = octetos[3] + "." + octetos[2] + "." + octetos[1] + "." + octetos[0] + ".in-addr.arpa";
                ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_PTRType WHERE ContainerName = '" + container + "' AND OwnerName='" + ownerName + "'");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(_managementScopes[0], query);

                ManagementObjectCollection recordCollection;
                using (recordCollection = searcher.Get())
                {
                    var queryMo = from ManagementObject mo in recordCollection select mo;
                    Parallel.ForEach(queryMo, record => records.Add(MsDnsPtrRecord.Parse(record)));
                }
            }
            catch (Exception)
            {
                records = new List<MsDnsPtrRecord>();
            }
            return records;
        }

        public void DeleteRecord(MsDnsRecord record)
        {
            string type;

            if (record is MsDnsARecord)
            {
                type = "MicrosoftDNS_AType";
            }
            else if (record is MsDnsMxRecord)
            {
                type = "MicrosoftDNS_MXType";
            }
            else if (record is MsDnsCnameRecord)
            {
                type = "MicrosoftDNS_CNAMEType";
            }
            else if (record is MsDnsNsRecord)
            {
                type = "MicrosoftDNS_NSType";
            }
            else if (record is MsDnsTxtRecord)
            {
                type = "MicrosoftDNS_TXTType";
            }
            else if (record is MsDnsPtrRecord)
            {
                type = "MicrosoftDNS_PTRType";
            }
            else if (record is MsDnsSrvRecord)
            {
                type = "MicrosoftDNS_SRVType";
            }
            else
            {
                throw new NotSupportedException("Derrived DNS record type is not supported.");
            }

            try
            {
                ObjectQuery query;
                if (!string.IsNullOrEmpty(record.Value))
                {
                    query = new ObjectQuery("SELECT * FROM " + type + " " +
                                            "WHERE OwnerName = '" + record.FullName + "' " +
                                            "AND RecordData = '" + record.Value + "' ");
                }
                else
                {
                    query = new ObjectQuery("SELECT * FROM " + type + " " +
                                            "WHERE OwnerName = '" + record.FullName + "' ");
                }

                ManagementObjectSearcher searcher0 = new ManagementObjectSearcher(_managementScopes[0], query);
                ManagementObjectCollection recordCollection0 = searcher0.Get();

                var registerExist = false;
                foreach (ManagementObject item in recordCollection0)
                {
                    item.Delete();
                    registerExist = true;
                }
                ////-------------------------------------------------------------------------------------------------------------
                //ManagementObjectSearcher searcher1 = new ManagementObjectSearcher(_managementScopes[1], query);
                //ManagementObjectCollection recordCollection1 = searcher1.Get();

                //foreach (ManagementObject item in recordCollection1)
                //{
                //    item.Delete();
                //    registerExist = true;
                //}
                ////-------------------------------------------------------------------------------------------------------------
                //ManagementObjectSearcher searcher2 = new ManagementObjectSearcher(_managementScopes[2], query);
                //ManagementObjectCollection recordCollection2 = searcher2.Get();

                //foreach (ManagementObject item in recordCollection2)
                //{
                //    item.Delete();
                //    registerExist = true;
                //}
                ////-------------------------------------------------------------------------------------------------------------
                //ManagementObjectSearcher searcher3 = new ManagementObjectSearcher(_managementScopes[3], query);
                //ManagementObjectCollection recordCollection3 = searcher3.Get();

                //foreach (ManagementObject item in recordCollection3)
                //{
                //    item.Delete();
                //    registerExist = true;
                //}
            }
            catch (Exception exception)
            {
                _logger.Log(string.Format("{0}, exception: {1}", LogUtils.GetCurrentMethod(this), exception));
                throw exception;
            }
        }
    }
}
