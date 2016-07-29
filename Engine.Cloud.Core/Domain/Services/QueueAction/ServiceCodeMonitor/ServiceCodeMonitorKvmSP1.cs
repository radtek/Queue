using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public class ServiceCodeMonitorKvmSP1 : ServiceCodeMonitorXenSP1
    {
        public ServiceCodeMonitorKvmSP1()
        {
            UrlBase = string.Format("{0}kratos/rest/", ConfigurationManager.AppSettings.Get("Cloud.KvmSP1.UrlWS"));
        }
    }
}
