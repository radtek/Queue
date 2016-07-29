using System.Collections.Generic;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Model.ServerHosts;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine
{
    public interface IVirtualMachineService
    {
        ServiceResult Install(Client client, byte vcpu, int frequency, int memory, string image, int diskSize, decimal[] bandwidths, int partitions, string formatDisk, long serverId);
        
        void Load(Server server);

        Server Load(string hypervisorIdentifier);

        void LoadAll(List<Server> servers);

        ServiceResult Uninstall(Server server);

        ServiceResult Reinstall(Server server, Image image);

        ServiceResult PowerOn(Server server);

        ServiceResult PowerOff(Server server);

        ServiceResult Reboot(Server server);

        ServiceResult Suspend(Server server);

        ServiceResult Unlock(Server server);

        ServiceResult Resume(Server server);

        ServiceResult Reset(Server server);

        ServiceResult Pause(Server server);

        ServiceResult ShutDown(Server server);

        ServiceResult Refresh(Server server);
    }
}