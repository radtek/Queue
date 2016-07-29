using System;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.VMMService.DataContract;
using Engine.Cloud.Core.Utils.VMMService;

namespace Engine.Cloud.Core.Domain.Services.MemoryVcpu
{
    public class MemoryVcpuServiceHypervisorV : HyperVServiceBase, IMemoryVcpuService
    {
        private readonly Server _server;
        private readonly VMMClient _vmmClient;

        public MemoryVcpuServiceHypervisorV(Server server)
        {
            _server = server;
            _vmmClient = new VMMClient();
        }

        public ServiceResult Change(byte vcpu, int frequency, int memory)
        {
            try
            {
                VirtualmachineContract virtualmachineContract = _vmmClient.MemoryVcpuUpdate(_server.HypervisorIdentifier, vcpu, memory);

                if (string.IsNullOrWhiteSpace(virtualmachineContract.ID))
                    return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.FAILED };
            }
            catch (Exception e)
            {
                _logger.Log(e);
                return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.FAILED };
            }

            return new ServiceResult { Result = _server.HypervisorIdentifier, Status = StatusService.SUCCESS };
        }
    }
}