using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine
{
    public class VirtualMachineServiceKvmRj : VirtualMachineServiceKvmSp
    {
        public VirtualMachineServiceKvmRj()
        {
            UrlBase = ConfigurationManager.AppSettings.Get("Cloud.KvmRJ1.UrlWS");
        }
    }
}