using System;
using Engine.Cloud.Core.Model;

namespace Engine.Cloud.Core.Domain.Services.MemoryVcpu
{
    public class MemoryVcpuServiceFactory
    {
        public IMemoryVcpuService GetInstance(Server server)
        {
            switch (server.Image.TypeHipervisorId)
            {
                case (int)TypeHypervisor.XEN_SP1:
                    return new MemoryVcpuServiceXenSp(server);
                case (int)TypeHypervisor.XEN_RJ1:
                    return new MemoryVcpuServiceXenRj(server);
                case (int)TypeHypervisor.KVM_SP1:
                    return new MemoryVcpuServiceKvmSp(server);
                case (int)TypeHypervisor.HYPERV_SP1:
                    return new MemoryVcpuServiceHypervisorV(server);
                case (int)TypeHypervisor.KVM_RJ1:
                    return new MemoryVcpuServiceKvmRj(server);
            }

            throw new Exception("Hypervisor inválido");
        }
    }
}
