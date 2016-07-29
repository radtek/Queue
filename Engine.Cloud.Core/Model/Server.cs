using Engine.Cloud.Core.Utils;
using System;

namespace Engine.Cloud.Core.Model
{
    public partial class Server : ClonableObject
    {
        public RemoteStatus RemoteStatus { get; set; } = RemoteStatus.DontExist;
        public Resources Resources { get; set; }
        public Guid ServiceCode { get; set; } = Guid.NewGuid();
        
        public string GetServerName()
        {
            return (string.IsNullOrEmpty(NickName) ? Name : NickName);
        }

        public bool Blocked()
        {
            return (this.StatusBlockId != (int)StatusBlock.Nothing);
        }

        public TypeHypervisor GetHypervisor()
        {
            switch (this.Image.TypeHipervisorId)
            {
                case (int)TypeHypervisor.HYPERV_SP1:
                    return TypeHypervisor.HYPERV_SP1;
                case (int)TypeHypervisor.KVM_SP1:
                    return TypeHypervisor.KVM_SP1;
                case (int)TypeHypervisor.KVM_RJ1:
                    return TypeHypervisor.KVM_RJ1;
                case (int)TypeHypervisor.XEN_RJ1:
                    return TypeHypervisor.XEN_RJ1;
                case (int)TypeHypervisor.XEN_SP1:
                    return TypeHypervisor.XEN_SP1;
                default:
                    throw new Exception();
            }
        }
    }
}
