using Engine.Cloud.Core.Model.DataContext;
using System;
using System.Data;
using System.ServiceProcess;
using System.Threading;
using Utils;

namespace Engine.Cloud.QueueAction
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var thread = new Thread(Run);
                thread.Start();
            }
            catch (Exception e)
            {
                LogFactory.GetInstance().Log(e);
            }
        }

        protected override void OnStop()
        {

        }

        private void Run()
        {
            while (true)
            {
                try
                {
                    using (var context = new EngineCloudDataContext())
                    {
                        context.Database.Connection.Open();

                        if (context.Database.Connection.State == ConnectionState.Open)
                        {
                            var actionDomain = new QueueActionMonitor(context);

                            var total = actionDomain.Run();

                            if (total > 10)
                            {
                                LogFactory.GetInstance().Log(string.Format("{0} em processamento.", total));
                                
                                continue;
                            }

                            Thread.Sleep(10 * 1000);
                        }
                        else
                        {
                            context.Database.Connection.Close();
                            throw new InvalidOperationException("[QueueAction] erro ao abrir conexao com banco de dados");
                        }
                    }
                }
                catch (Exception e)
                {
                    LogFactory.GetInstance().Log("[QueueAction] erro ao processar itens da fila.", e);
                    Thread.Sleep(120 * 1000);
                }
            }
        }
    }
}
