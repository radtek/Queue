using System;
using Engine.Cloud.Core.Model;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine
{
    public class VirtualMachineServiceFactory
    {
        public IVirtualMachineService GetInstance(Server server)
        {
            switch (server.Image.TypeHipervisorId)
            {
                case (int)TypeHypervisor.XEN_SP1:
                    return new VirtualMachineServiceXenSp();
                case (int)TypeHypervisor.XEN_RJ1:
                    return new VirtualMachineServiceXenRj();
                case (int)TypeHypervisor.KVM_SP1:
                    return new VirtualMachineServiceKvmSp();
                case (int)TypeHypervisor.HYPERV_SP1:
                    return new VirtualMachineServiceHyperV();
                case (int)TypeHypervisor.KVM_RJ1:
                    return new VirtualMachineServiceKvmRj();
            }

            throw new Exception("Hypervisor inválido");
        }

        public IVirtualMachineService GetInstance(Image image)
        {
            switch (image.TypeHipervisorId)
            {
                case (int)TypeHypervisor.XEN_SP1:
                    return new VirtualMachineServiceXenSp();
                case (int)TypeHypervisor.XEN_RJ1:
                    return new VirtualMachineServiceXenRj();
                case (int)TypeHypervisor.KVM_SP1:
                    return new VirtualMachineServiceKvmSp();
                case (int)TypeHypervisor.HYPERV_SP1:
                    return new VirtualMachineServiceHyperV();
                case (int)TypeHypervisor.KVM_RJ1:
                    return new VirtualMachineServiceKvmRj();
            }

            throw new Exception("Hypervisor inválido");
        }

        public IVirtualMachineService GetInstance(TypeHypervisor typeHypervisor)
        {
            switch (typeHypervisor)
            {
                case TypeHypervisor.XEN_SP1:
                    return new VirtualMachineServiceXenSp();
                case TypeHypervisor.XEN_RJ1:
                    return new VirtualMachineServiceXenRj();
                case TypeHypervisor.KVM_SP1:
                    return new VirtualMachineServiceKvmSp();
                case TypeHypervisor.HYPERV_SP1:
                    return new VirtualMachineServiceHyperV();
                case TypeHypervisor.KVM_RJ1:
                    return new VirtualMachineServiceKvmRj();
            }

            throw new Exception("Hypervisor inválido");
        }
    }
}
