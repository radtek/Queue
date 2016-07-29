using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public interface IIPServerService
    {
        ServiceResult GetNetworkDetailsByMac(string mac);
        ServiceResult CreateAdditionalIP(string networkInterfaceName);
        ServiceResult DeleteAdditionalIP(string ip);
        ServiceResult CreateVip(Server serverTarget, string newIp, string networkInterfaceName);
        ServiceResult LoadNetworkInterface(string macAddress);
        ServiceResult CreateNetworkinterface(decimal bandwidth, int vlanId, string zabbixTemplate);
        ServiceResult ConfigureNetworkinterface(string macAddress, decimal bandwidth, string zabbixTemplate, int vlanId);
        ServiceResult UpdateNetworkInterface(string macAddress, decimal bandwidth, string zabbixTemplate, string serverName);
        ServiceResult DeleteNetworkinterface(string macAddress);
    }
}