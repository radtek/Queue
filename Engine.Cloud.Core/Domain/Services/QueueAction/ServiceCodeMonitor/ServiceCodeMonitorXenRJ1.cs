using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public class ServiceCodeMonitorXenRJ1 : ServiceCodeMonitorXenSP1
    {
        public ServiceCodeMonitorXenRJ1()
        {
            UrlBase = string.Format("{0}cloud/rest/", ConfigurationManager.AppSettings.Get("Cloud.XenRJ1.UrlWS"));
        }
    }
}