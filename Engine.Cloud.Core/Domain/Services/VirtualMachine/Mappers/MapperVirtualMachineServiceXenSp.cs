using System;
using System.Collections.Generic;
using System.Xml;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Utils.Extensions;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine.Mappers
{
    public class MapperVirtualMachineServiceXenSp : IMapperVirtualMachineService
    {
        private readonly XmlDocument _xmlDocument;
        private long _serverId;
        private readonly ServerDomain _serverDomain;

        public MapperVirtualMachineServiceXenSp(object @object)
        {
            _xmlDocument = (XmlDocument)@object;
            _serverDomain = new ServerDomain(new EngineCloudDataContext());
        }

        public void MapperToServer(Server server)
        {
            _serverId = server.ServerId;
            server.RemoteStatus = GetHypervisorStatus();

            if (server.RemoteStatus == RemoteStatus.DontExist)
                return;

            server.Name = GetName();
            server.HypervisorIdentifier = GetHypervisorIdentifier();
            GetRemoteLock(server);
            server.Resources = new Resources();
            server.Resources.Vcpu = GetVcpu();
            server.Resources.Frequency = GetFrequency();
            server.Resources.Memory = GetMemory();
         
            server.Resources.NetworkInterfaces = GetNetworkInterfaces();
            
            //    new List<NetworkInterface>();
            //server.Resources.NetworkInterfaces.Add(new NetworkInterface());
            //server.Resources.NetworkInterfaces[0].Name = GetNetworkInterfaceName();
            //server.Resources.NetworkInterfaces[0].BandWidth = GetBandWidth();
            //server.Resources.NetworkInterfaces[0].Mac = GetMacAddress();
            //server.Resources.NetworkInterfaces[0].Ips = GetIps();
            //server.Resources.NetworkInterfaces[0].Firewalls = GetFirewalls();
            //server.Resources.NetworkInterfaces[0].Vlan = GetVlan();
        }

       
        private void GetRemoteLock(Server server)
        {
            if ((_xmlDocument.SelectSingleNode("/virtualmachine/state") != null) &&
                (_xmlDocument.SelectSingleNode("/virtualmachine/state").InnerText == "UNKNOWN"))
            {
                server.StatusBlockId = (int)StatusBlock.Unknown;
                server.MessageStatus = StatusBlock.Unknown.GetDescription();
            }
            else if (_xmlDocument.InnerXml.Contains("virtualmachine lock=\"MAINTENANCE\"") || _xmlDocument.InnerXml.Contains("virtualmachine lock=\"EXECUTION\""))
            {
                server.StatusBlockId = (int)StatusBlock.Maintenance;
                server.MessageStatus = StatusBlock.Maintenance.GetDescription();
            }
            else if (_xmlDocument.InnerXml.Contains("virtualmachine lock=\"SUSPENDED\""))
            {
                server.StatusBlockId = (int)StatusBlock.Suspend;
                server.MessageStatus = StatusBlock.Suspend.GetDescription();
            }
            else if (server.StatusBlockId == (int)StatusBlock.Busy)
            {
                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = StatusBlock.Busy.GetDescription();
            }
            else
            {
                if (server.StatusBlockId == (int)StatusBlock.Nothing)
                    return;

                // retira qualquer bloqueio.
                var server1 = _serverDomain.Get(x => x.ServerId == server.ServerId);
                server1.StatusBlockId = (int)StatusBlock.Nothing;
                server1.MessageStatus = string.Empty;
                _serverDomain.AddUpdateServer(server1);
            }
        }

        public void MapperToServer(Server server, Guid serviceCode)
        {
            server.Name = GetName();
            server.HypervisorIdentifier = GetHypervisorIdentifier();
        }

        public void MapperToServers(List<Server> servers)
        {
            XmlNodeList virtualmachines = _xmlDocument.SelectNodes("/collection/virtualmachine");
            if (virtualmachines != null)
                foreach (XmlNode xmlNodeVirtualMachine in virtualmachines)
                {
                    var server = new Server();

                    server.HypervisorIdentifier = xmlNodeVirtualMachine.SelectSingleNode("identifier").InnerText; ;
                    server.Name = xmlNodeVirtualMachine.SelectSingleNode("name").InnerText;

                    if (xmlNodeVirtualMachine.SelectSingleNode("state").InnerText != null)
                    {
                        switch (xmlNodeVirtualMachine.SelectSingleNode("state").InnerText)
                        {
                            case "POWERED_ON":
                                server.RemoteStatus = RemoteStatus.Online;
                                break;
                            case "SUSPENDED":
                                server.RemoteStatus = RemoteStatus.Suspended;
                                break;
                            case "POWERED_OFF":
                                server.RemoteStatus = RemoteStatus.Offline;
                                break;
                        }
                    }

                    servers.Add(server);
                }
        }

        private string GetName()
        {
            if (_xmlDocument.SelectSingleNode("/virtualmachine/name") != null)
                return _xmlDocument.SelectSingleNode("/virtualmachine/name").InnerText;
            throw new Exception(string.Format("Informação de nome NÃO carregado. serverdId : {0}", _serverId));
        }

        private RemoteStatus GetHypervisorStatus()
        {
            if (_xmlDocument.SelectSingleNode("/virtualmachine/state") != null)
            {
                switch (_xmlDocument.SelectSingleNode("/virtualmachine/state").InnerText)
                {
                    case "POWERED_ON":
                        return RemoteStatus.Online;
                    case "SUSPENDED":
                        return RemoteStatus.Suspended;
                    case "POWERED_OFF":
                        return RemoteStatus.Offline;
                    default:
                        return RemoteStatus.DontExist;
                }
            }

            return RemoteStatus.DontExist;
        }

        private string GetHypervisorIdentifier()
        {
            if (_xmlDocument.SelectSingleNode("/virtualmachine/identifier") != null)
                return _xmlDocument.SelectSingleNode("/virtualmachine/identifier").InnerText;
            throw new Exception(string.Format("Informação de identifier NÃO carregada. serverdId : {0}", _serverId));
        }

        private int GetVcpu()
        {
            if (_xmlDocument.SelectSingleNode("/virtualmachine/vcpu") != null)
                return Convert.ToInt32(_xmlDocument.SelectSingleNode("/virtualmachine/vcpu").InnerText);
            throw new Exception(string.Format("Informação de Vcpu NÃO carregada. serverdId : {0}", _serverId));
        }

        private int GetFrequency()
        {
            if (_xmlDocument.SelectSingleNode("/virtualmachine/frequency") != null)
                return Convert.ToInt32(_xmlDocument.SelectSingleNode("/virtualmachine/frequency").InnerText);
            throw new Exception(string.Format("Informação de Frequência NÃO carregada. serverdId : {0}", _serverId));
        }

        private int GetMemory()
        {
            if (_xmlDocument.SelectSingleNode("/virtualmachine/memory") != null)
                return Convert.ToInt32(_xmlDocument.SelectSingleNode("/virtualmachine/memory").InnerText);
            throw new Exception(string.Format("Informação de Memória não carregada. serverdId : {0}", _serverId));
        }
        

        private List<NetworkInterface> GetNetworkInterfaces()
        {
            XmlNodeList networkInterfaces = _xmlDocument.SelectNodes("/virtualmachine/networkinterfaces/networkinterface");
            var list = new List<NetworkInterface>();
            if (networkInterfaces != null)
                foreach (XmlNode xmlNodeNetworkInterface in networkInterfaces)
                {
                    var networkInterface = new NetworkInterface();

                    networkInterface.Name = GetNetworkInterfaceName(xmlNodeNetworkInterface);
                    networkInterface.BandWidth = GetBandWidth(xmlNodeNetworkInterface);
                    networkInterface.Mac = GetMacAddress(xmlNodeNetworkInterface);
                    networkInterface.Ips = GetIps(xmlNodeNetworkInterface);
                    networkInterface.Type = networkInterface.Vlan != null ? (networkInterface.Vlan.Contains("virtbr") ? "Pública" : "Privada") : "Pública";
                 
                    networkInterface.Vlan = GetVlan(xmlNodeNetworkInterface);

                    list.Add(networkInterface);
                }

            if (list.Count > 0)
                return list;
            throw new Exception(string.Format("Informação de REDE NÃO carregada. serverdId : {0}", _serverId));
        }

        private TypeDisk GetDiskType(XmlNode xmlNodeDisk)
        {
            var nodeTypeList = xmlNodeDisk.SelectNodes("diskType");
            string type = "SO";
            if (nodeTypeList != null)
                if (nodeTypeList.Count > 0)
                    type = nodeTypeList[0].InnerText;

            switch (type)
            {
                case "DATA":
                    return TypeDisk.Data;
                case "SO":
                    return TypeDisk.SO;
                default:
                    throw new Exception(string.Format("Informação de tipo de disco NÃO carregada. serverdId : {0}", _serverId));
            }
        }

        private string GetNetworkInterfaceName(XmlNode xmlNodenetworkInterface)
        {
            if (xmlNodenetworkInterface.SelectSingleNode("name") != null)
                return xmlNodenetworkInterface.SelectSingleNode("name").InnerText;
            return string.Empty;
        }

        private decimal GetBandWidth(XmlNode xmlNodenetworkInterface)
        {
            decimal bandWidth = 0;

            var nodebandwidth = xmlNodenetworkInterface.SelectSingleNode("./bandwidth");
            if (nodebandwidth != null && nodebandwidth.HasChildNodes)
            {
                XmlNode node = xmlNodenetworkInterface.SelectSingleNode("./privateRange");
                if (node == null)
                {
                    bandWidth += Convert.ToDecimal(nodebandwidth.InnerText.Replace('.', ','));
                }
            }

            return bandWidth;
        }

        private string GetMacAddress(XmlNode xmlNodenetworkInterface)
        {
            if (xmlNodenetworkInterface.SelectSingleNode("mac") != null)
                return xmlNodenetworkInterface.SelectSingleNode("mac").InnerText;
            return string.Empty;
        }

        private string GetVlan(XmlNode xmlNodenetworkInterface)
        {
            if (xmlNodenetworkInterface.SelectSingleNode("vlan") != null)
                return xmlNodenetworkInterface.SelectSingleNode("vlan/name").InnerText;
            return string.Empty;
        }

        private List<Ip> GetIps(XmlNode xmlNodenetworkInterface)
        {
            var list = new List<Ip>();

            var ip = new Ip
            {
                Number = xmlNodenetworkInterface.SelectSingleNode("ipv4") == null
                        ? String.Empty
                        : xmlNodenetworkInterface.SelectSingleNode("ipv4").InnerText,
                Type = TypeIp.Principal
            };
            list.Add(ip);

            var interfaceName = xmlNodenetworkInterface.SelectSingleNode("name") == null
                ? string.Empty
                : xmlNodenetworkInterface.SelectSingleNode("name").InnerText;
            var nodeAdditionalIps = xmlNodenetworkInterface.SelectSingleNode("additionalips");
            if (nodeAdditionalIps != null && nodeAdditionalIps.HasChildNodes)
            {
                foreach (XmlNode nodeIp in nodeAdditionalIps)
                {
                    if (nodeIp != null && !string.IsNullOrEmpty(nodeIp.InnerText))
                    {
                        if (nodeIp.SelectSingleNode("ipv4") != null)
                        {
                            var ipAdditional = new Ip
                            {
                                Number = nodeIp.SelectSingleNode("ipv4").InnerText,
                                Vip = GetVip(nodeIp, interfaceName),
                                Type = TypeIp.Additional
                            };
                            list.Add(ipAdditional);
                        }
                    }
                }
            }

            if (list.Count > 0)
                return list;
            return new List<Ip>();
        }

        private static string GetVip(XmlNode nodeIp, string interfaceName)
        {
            if (nodeIp.Attributes != null && nodeIp.Attributes["vip"] == null)
                return string.Empty;

            if (nodeIp.Attributes["vip"].Value != interfaceName)
                return "exist_vip";
            return "is_vip";
        }

     
       
    }
}