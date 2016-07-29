using Engine.Cloud.Core.Model;
using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.MemoryVcpu
{
    public class MemoryVcpuServiceKvmRj : MemoryVcpuServiceKvmSp
    {
        public MemoryVcpuServiceKvmRj(Server server) : base(server)
        {
            UrlBase = ConfigurationManager.AppSettings.Get("Cloud.KvmRJ1.UrlWS");
        }
    }
}