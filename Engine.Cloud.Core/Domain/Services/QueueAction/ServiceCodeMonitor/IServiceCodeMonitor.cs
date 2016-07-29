using Engine.Cloud.Core.Model.Results;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public interface IServiceCodeMonitor
    {
        ServiceResult Get(string serviceCode);
    }
}
