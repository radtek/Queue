using Engine.Cloud.Core.Model.Results;

namespace Engine.Cloud.Core.Domain.Services.MemoryVcpu
{
    public interface IMemoryVcpuService
    {
        ServiceResult Change(byte vcpu, int frequency, int memory);
    }
}