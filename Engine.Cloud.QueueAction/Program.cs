using System;
using System.ServiceProcess;
using log4net.Config;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.QueueAction
{
    static class Program
    {
        static void Main()
        {
            XmlConfigurator.Configure();

            if (Environment.UserInteractive)
            {
                QueueActionMonitor queueActionMonitor = new QueueActionMonitor(new EngineCloudDataContext());
                queueActionMonitor.Run();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new Service()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
