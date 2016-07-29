using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Cloud.Core.Domain.Services.AdditionalIP;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Logging;
using Engine.Cloud.Panel.Utils;
using Utils;


namespace Engine.Cloud.Core.Domain.Services.VirtualMachine.Mappers
{
    public class MapperVirtualMachineServiceHyperV : IMapperVirtualMachineService
    {
        private readonly ServiceReferenceVmm.VMM _contextVmm;
        private ServiceReferenceVmm.VirtualMachine _virtualMachine;
        private const int _frenquencyHyperV = 3600;
        private readonly double CACHE_EXPIRATION_TIME = AppSettings.GetDouble("CACHE-EXPIRATION-60");
        private readonly ILogger _logger;

        public MapperVirtualMachineServiceHyperV(object @object)
        {
            _contextVmm = (ServiceReferenceVmm.VMM)@object;
            _logger = LogFactory.GetInstance();
        }

        public void MapperToServer(Server server)
        {
            _virtualMachine = _contextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).FirstOrDefault();

            if (_virtualMachine == null)
            {
                server.RemoteStatus = RemoteStatus.DontExist;
                return;
            }

            //#if !DEBUG
            _contextVmm.LoadProperty(_virtualMachine, "VirtualNetworkAdapters");
            //#endif

            _contextVmm.LoadProperty(_virtualMachine, "VirtualHardDisks");
            _contextVmm.LoadProperty(_virtualMachine, "VirtualDiskDrives");

            server.Name = _virtualMachine.Name;
            server.HypervisorIdentifier = _virtualMachine.ID.ToString();
            server.RemoteStatus = GetHypervisorStatus();

            server.Resources = new Resources();
            server.Resources.Vcpu = int.Parse(_virtualMachine.CPUCount.ToString());
            server.Resources.FrequencyNominal = _virtualMachine.CPUType;
            server.Resources.Frequency = _frenquencyHyperV;
            server.Resources.Memory = long.Parse(_virtualMachine.Memory.ToString()) * 1024;

          

            server.Resources.NetworkInterfaces = LoadNetworkInterfaces(server);
        }

        private List<NetworkInterface> LoadNetworkInterfaces(Server server)
        {
            var interfaces = new List<NetworkInterface>();
            var key = string.Empty;

            try
            {
                var ipServerService = new IPServiceFactory().GetInstance(server);

                foreach (var item in _virtualMachine.VirtualNetworkAdapters)
                {
                    if (string.IsNullOrEmpty(item.MACAddress))
                        continue;

                    key = string.Format("api/virtualmachine/{0}/networkinterface?macAddress={1}", server.HypervisorIdentifier, item.MACAddress);

                    ServiceResult serviceResult;
                    NetworkInterface networkInterface = new NetworkInterface() { Mac = item.MACAddress };

                    if (server.StatusBlockId == (int)StatusBlock.Busy)
                        MemoryCacheManager.Remove(key);

                    if (MemoryCacheManager.Exists(key))
                    {
                        serviceResult = (ServiceResult)MemoryCacheManager.Get(key);
                    }
                    else
                    {
                        serviceResult = ipServerService.LoadNetworkInterface(item.MACAddress);
                        if (serviceResult != null)
                        {
                            MemoryCacheManager.AddOrUpdate(key, serviceResult, 0, CACHE_EXPIRATION_TIME);
                        }
                    }

                    if (serviceResult != null && serviceResult.Results.Any() && !string.IsNullOrEmpty(serviceResult.Results["ipv4"]))
                    {
                        networkInterface.Ips.Add(new Ip
                        {
                            Number = serviceResult.Results["ipv4"],
                            Type = TypeIp.Principal,
                        });

                        networkInterface.BandWidth = Convert.ToDecimal(serviceResult.Results["bandwidth"].Replace('.', ','));
                    }
                    else
                    {
                        foreach (var ipv4Addresses in item.IPv4Addresses)
                        {
                            networkInterface.Ips.Add(new Ip
                            {
                                Number = ipv4Addresses,
                                Type = TypeIp.Principal
                            });
                            networkInterface.BandWidth = 0;
                        }
                    }

                    if (serviceResult != null && serviceResult.Results.Any() && serviceResult.Results.ContainsKey("name"))
                        networkInterface.Name = serviceResult.Results["name"];

                    if (serviceResult != null && serviceResult.Results.Any() && serviceResult.Results.ContainsKey("vlanid"))
                        networkInterface.Vlan = serviceResult.Results["vlanid"];

                    networkInterface.Type = networkInterface.BandWidth > 0 ? "Pública" : "Privada";
                    interfaces.Add(networkInterface);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this) + " - url:" + key, ex);
            }

            return interfaces;
        }

     

        private RemoteStatus GetHypervisorStatus()
        {
            switch (_virtualMachine.StatusString.ToLower())
            {
                case "running":
                    return RemoteStatus.Online;
                case "stopped":
                    return RemoteStatus.Offline;
                case "undercreation":
                    return RemoteStatus.Offline;
                case "creationfailed":
                    return RemoteStatus.Offline;
                case "paused":
                    return RemoteStatus.Suspended;
                default:
                    return RemoteStatus.DontExist;
            }
        }
    }
}