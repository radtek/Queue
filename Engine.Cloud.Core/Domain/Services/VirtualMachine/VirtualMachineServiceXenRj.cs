using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine
{
    public class VirtualMachineServiceXenRj : VirtualMachineServiceXenSp
    {
        public VirtualMachineServiceXenRj()
        {
            UrlBase = ConfigurationManager.AppSettings.Get("Cloud.XenRJ1.UrlWS");
        }
    }
}