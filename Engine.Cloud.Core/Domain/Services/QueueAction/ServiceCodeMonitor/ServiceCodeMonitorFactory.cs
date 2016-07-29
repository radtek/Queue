using System;

using Engine.Cloud.Core.Model;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public class ServiceCodeMonitorFactory
    {
        public IServiceCodeMonitor GetInstance(Server server)
        {
            switch (server.Image.TypeHipervisorId)
            {
                case (int)TypeHypervisor.HYPERV_SP1:
                    return new ServiceCodeMonitorHyperV();
                case (int)TypeHypervisor.XEN_SP1:
                    return new ServiceCodeMonitorXenSP1();
                case (int)TypeHypervisor.XEN_RJ1:
                    return new ServiceCodeMonitorXenRJ1();
                case (int)TypeHypervisor.KVM_SP1:
                    return new ServiceCodeMonitorKvmSP1();
                case (int)TypeHypervisor.KVM_RJ1:
                    return new ServiceCodeMonitorKvmRJ1();
            }
            throw new Exception("hypervisor inválido");
        }

        public IServiceCodeMonitor GetInstance(TypeHypervisor typeHypervisor)
        {
            switch (typeHypervisor)
            {
                case TypeHypervisor.HYPERV_SP1:
                    return new ServiceCodeMonitorHyperV();
                case TypeHypervisor.XEN_SP1:
                    return new ServiceCodeMonitorXenSP1();
                case TypeHypervisor.XEN_RJ1:
                    return new ServiceCodeMonitorXenRJ1();
                case TypeHypervisor.KVM_SP1:
                    return new ServiceCodeMonitorKvmSP1();
            }
            throw new Exception("hypervisor inválido");
        }
    }
}
