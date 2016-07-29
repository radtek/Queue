using System;
using Engine.Cloud.Core.Model;

namespace Engine.Cloud.Core.Domain
{
    public class MonitoringDomain
    {
        public string GetInstance(Server server)
        {
            //switch (server.TypeHypervisor)
            //{
            //    case TypeHypervisor.XEN_SP1:
                    
            //    case TypeHypervisor.XEN_RJ1:
                    
            //    case TypeHypervisor.KVM_SP1:
                    
            //    case TypeHypervisor.HYPERV_SP1:
                    
            //}

            throw new Exception("Hypervisor inválido");
        }
    }
}
