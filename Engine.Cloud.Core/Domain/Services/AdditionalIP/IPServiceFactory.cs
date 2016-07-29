using Engine.Cloud.Core.Model;
using System;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public class IPServiceFactory
    {
        public IIPServerService GetInstance(Server server)
        {
            switch (server.Image.TypeHipervisorId)
            {
                case (int)TypeHypervisor.XEN_SP1:
                    return new IPServerServiceXenSp(server);
                case (int)TypeHypervisor.XEN_RJ1:
                    return new IPServerServiceXenRj(server);
                case (int)TypeHypervisor.KVM_SP1:
                    return new IPServerServiceKvmSp(server);
                case (int)TypeHypervisor.HYPERV_SP1:
                    return new IPServiceHyperV(server);
                case (int)TypeHypervisor.KVM_RJ1:
                    return new IpServerServiceKvmRj(server);
            }

            throw new Exception("Hypervisor inválido");
        }
    }
}
