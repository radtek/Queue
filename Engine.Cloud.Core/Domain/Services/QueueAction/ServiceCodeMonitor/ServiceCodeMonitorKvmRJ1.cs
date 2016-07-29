using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public class ServiceCodeMonitorKvmRJ1 : ServiceCodeMonitorXenSP1
    {
        public ServiceCodeMonitorKvmRJ1()
        {
            UrlBase = string.Format("{0}kratos/rest/", ConfigurationManager.AppSettings.Get("Cloud.KvmRJ1.UrlWS"));
        }
    }
}
