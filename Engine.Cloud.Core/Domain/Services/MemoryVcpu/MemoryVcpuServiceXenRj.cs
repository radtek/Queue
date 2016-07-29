using Engine.Cloud.Core.Model;
using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.MemoryVcpu
{
    public class MemoryVcpuServiceXenRj : MemoryVcpuServiceXenSp
    {
        public MemoryVcpuServiceXenRj(Server server) : base(server)
        {
            UrlBase = ConfigurationManager.AppSettings.Get("Cloud.XenRJ1.UrlWS");
        }
    }
}