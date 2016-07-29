using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using System;
using Engine.Cloud.Core.Utils.VMMService;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public class IPServiceHyperV : HyperVServiceBase, IIPServerService
    {

        private readonly Server _server;
        private readonly VMMClient _vmmClient;

        public IPServiceHyperV(Server server)
        {
            _server = server;
            _vmmClient = new VMMClient();
        }

        public ServiceResult CreateNetworkinterface(decimal bandwidth, int vlanId, string zabbixTemplate)
        {
            try
            {
                _vmmClient.NetworkinterfaceCreate(_server.HypervisorIdentifier, _server.Name, _server.Client.CustomerCode, bandwidth, vlanId, zabbixTemplate);
            }
            catch (Exception e)
            {
                _logger.Log(e);
                return new ServiceResult { Result = e.Message, Status = StatusService.FAILED };
            }
            return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.SUCCESS };
        }

        public ServiceResult DeleteNetworkinterface(string macAddress)
        {
            try
            {
                _vmmClient.NetworkinterfaceDelete(_server.HypervisorIdentifier, macAddress);
            }
            catch (Exception e)
            {
                _logger.Log(e);
                return new ServiceResult { Result = e.Message, Status = StatusService.FAILED };
            }
            return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.SUCCESS };
        }

        public ServiceResult ConfigureNetworkinterface(string macAddress, decimal bandwidth, string zabbixTemplate, int vlanId)
        {
            try
            {
                _vmmClient.NetworkinterfaceConfigure(_server.HypervisorIdentifier, _server.Client.CustomerCode, macAddress, bandwidth, zabbixTemplate, _server.Name, vlanId);
            }
            catch (Exception e)
            {
                _logger.Log(e);
                return new ServiceResult { Result = e.Message, Status = StatusService.FAILED };
            }

            return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.SUCCESS };
        }

        public ServiceResult UpdateNetworkInterface(string macAddress, decimal bandwidth, string zabbixTemplate, string serverName)
        {
            try
            {
                _vmmClient.NetworkinterfaceUpdate(_server.HypervisorIdentifier, _server.Client.CustomerCode, macAddress, bandwidth, zabbixTemplate, serverName);
            }
            catch (Exception e)
            {
                _logger.Log(e);
                return new ServiceResult { Result = e.Message, Status = StatusService.FAILED };
            }

            return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.SUCCESS };
        }

        public ServiceResult CreateAdditionalIP(string networkInterfaceName)
        {
            throw new NotImplementedException();
        }

        public ServiceResult CreateVip(Server serverTarget, string newIp, string networkInterfacename)
        {
            throw new NotImplementedException();
        }

        public ServiceResult DeleteAdditionalIP(string ip)
        {
            throw new NotImplementedException();
        }

        public ServiceResult GetNetworkDetailsByMac(string mac)
        {
            throw new NotImplementedException();
        }

        public ServiceResult LoadNetworkInterface(string macAddress)
        {
            ServiceResult result = new ServiceResult();

            var virtualmachineNetworkContract = _vmmClient.GetNetworkinterface(_server.HypervisorIdentifier, macAddress);

            if ((virtualmachineNetworkContract != null) && (!string.IsNullOrEmpty(virtualmachineNetworkContract.MacAddress)))
            {
                if (!string.IsNullOrWhiteSpace(virtualmachineNetworkContract.Ipv4))
                    result.Results.Add("ipv4", virtualmachineNetworkContract.Ipv4);
                else
                    result.Results.Add("ipv4", string.Empty);

                if (!string.IsNullOrWhiteSpace(virtualmachineNetworkContract.Name))
                    result.Results.Add("name", virtualmachineNetworkContract.Name);

                if (!string.IsNullOrWhiteSpace(virtualmachineNetworkContract.Bandwidth))
                    result.Results.Add("bandwidth", virtualmachineNetworkContract.Bandwidth);

                if (!string.IsNullOrWhiteSpace(virtualmachineNetworkContract.VlanId))
                    result.Results.Add("vlanid", virtualmachineNetworkContract.VlanId);
            }
            else
                return null;

            return result;
        }
    }
}