using System;
using System.Linq;
using Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Requests;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Logging;
using Utils;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain.Services.QueueAction
{
    public class ServerEngine
    {
        private readonly EngineCloudDataContext _context;
        private readonly ServerDomain _serverDomain;
        private readonly ClientDomain _clientDomain;
        private readonly ServiceCodeMonitorFactory _serviceCodeMonitorFactory;
        private readonly ILogger _logger;

        public ServerEngine(EngineCloudDataContext context)
        {
            _context = context;
            _serverDomain = new ServerDomain(context);
            _serviceCodeMonitorFactory = new ServiceCodeMonitorFactory();
            _logger = LogFactory.GetInstance();
            _clientDomain = new ClientDomain(context);
        }

        public QueueActionResult ExecuteStep(QueueActionStep step)
        {
            QueueActionResult stepResult = null;

            var serverId = step.QueueAction.QueueActionReference.ValueId;
            var server = _serverDomain.Get(x => x.ServerId == serverId);

            var user = GetActionUser(step); // busca o usuario que disparou a ação para gravar log

            StepRequest stepRequest = new StepRequest().Deserialize(step.RequestBody);

            if (!string.IsNullOrEmpty(server.HypervisorIdentifier))
                _serverDomain.Load(server);


            switch (stepRequest.TypeActionStepId)
            {
                case (int)TypeActionStep.Server_InstallServer:
                    {
                        var installServerRequest = new InstallServerRequest
                        {
                            CustomerCode = stepRequest.Params["customerCode"],
                            PlanId = Convert.ToInt32(stepRequest.Params["planId"]),
                            ImageId = Convert.ToInt32(stepRequest.Params["imageId"]),
                            TypeImageId = Convert.ToInt32(stepRequest.Params["typeImageId"]),
                            Vcpu = Convert.ToByte(stepRequest.Params["vcpu"]),
                            Frequency = Convert.ToInt32(stepRequest.Params["frequency"]),
                            Memory = Convert.ToInt32(stepRequest.Params["memory"]),
                            Disk = Convert.ToInt32(stepRequest.Params["disk"]),
                            BandWidths = InstallServerRequest.ParseBandwidths(stepRequest.Params["bandWidths"]),
                            TypeManagementId = Convert.ToInt32(stepRequest.Params["typeManagementId"]),
                            Partitions = Convert.ToInt32(stepRequest.Params["partitions"]),
                            FormatDisk = stepRequest.Params["formatDisk"],
                            ServerId = Convert.ToInt64(stepRequest.Params["serverId"]),
                        };

                        var serverDomain = new ServerDomain(_context);
                        stepResult = serverDomain.Install(installServerRequest);

                        break;
                    }
                case (int)TypeActionStep.Server_ReinstallServer:
                    {
                        var serverDomain = new ServerDomain(_context);
                        var imageId = Convert.ToInt32(stepRequest.Params["imageId"]);
                        var typeImageId = Convert.ToInt32(stepRequest.Params["typeImageId"]);
                        stepResult = serverDomain.Reinstall(server, imageId, typeImageId);
                        break;
                    }
                case (int)TypeActionStep.Server_UninstallServer:
                    {
                        var serverDomain = new ServerDomain(_context);
                        stepResult = serverDomain.Uninstall(server);

                        break;
                    }
                case (int)TypeActionStep.Server_PowerOffServer:
                    {
                        if (server.RemoteStatus == RemoteStatus.Online)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.PowerOff(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
                        break;
                    }
                case (int)TypeActionStep.Server_ShutdownServer:
                    {
                        if (server.RemoteStatus == RemoteStatus.Online)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.ShutDown(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
                case (int)TypeActionStep.Server_PowerOnServer:
                    {
                        var serverDomain = new ServerDomain(_context);

                        if (server.RemoteStatus == RemoteStatus.Suspended)
                            stepResult = serverDomain.Resume(server);
                        else if (server.RemoteStatus == RemoteStatus.Offline)
                            stepResult = serverDomain.PowerOn(server);
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
                case (int)TypeActionStep.Server_Refresh:
                    {
                        if (server.RemoteStatus == RemoteStatus.Online)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.Refresh(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
                case (int)TypeActionStep.Server_PauseServer:
                    {
                        if (server.RemoteStatus != RemoteStatus.Suspended)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.Pause(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
                case (int)TypeActionStep.Server_SupendServer:
                    {
                        if (server.StatusBlockId == (int)StatusBlock.Suspend)
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
                        else
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.Suspend(server);
                        }

                        break;
                    }

                case (int)TypeActionStep.Server_SuspendPanelServer:
                    {
                        server.StatusBlockId = (int)StatusBlock.Suspend;
                        server.MessageStatus = "Suspenso";
                        var serverDomain = new ServerDomain(_context);
                        serverDomain.AddUpdateServer(server);

                        stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
                        break;
                    }
                case (int)TypeActionStep.Server_ReactivateServer:
                    {
                        var serverDomain = new ServerDomain(_context);
                        stepResult = serverDomain.Reactivate(server);

                        if (stepResult.Status == StatusQueueAction.COMPLETED)
                        {
                            server.StatusId = (int)Status.Active;
                            serverDomain.AddUpdateServer(server);
                        }

                        break;
                    }
                case (int)TypeActionStep.Server_ResumeServer:
                    {
                        if (server.RemoteStatus == RemoteStatus.Suspended)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.Resume(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
                case (int)TypeActionStep.Server_ResetServer:
                    {
                        if (server.RemoteStatus == RemoteStatus.Online)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.Reset(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
                case (int)TypeActionStep.Server_RebootServer:
                    {
                        if (server.RemoteStatus == RemoteStatus.Online)
                        {
                            var serverDomain = new ServerDomain(_context);
                            stepResult = serverDomain.Reboot(server);
                        }
                        else
                            stepResult = new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };

                        break;
                    }
              
              
          
                case (int)TypeActionStep.Server_ChangeMemoryVcpu:
                    {
                        var vcpu = Convert.ToByte(stepRequest.Params["vcpu"]);
                        var frequency = Convert.ToInt32(stepRequest.Params["frequency"]);
                        var memory = Convert.ToInt32(stepRequest.Params["memory"]);

                        var memoryVcpuDomain = new MemoryVcpuDomain(_context);
                        stepResult = memoryVcpuDomain.Change(server, vcpu, frequency, memory);

                        break;
                    }
            
              
                case (int)TypeActionStep.Server_AdditionalIp:
                    {
                        var additionalIpDomain = new IpServerDomain();
                        var action = stepRequest.Params["action"];

                        switch (action)
                        {
                            case "deleteip":
                                {
                                    var ip = stepRequest.Params["ip"];
                                    stepResult = additionalIpDomain.DeleteAdditionalIP(server, ip);
                                    break;
                                }
                            case "createvip":
                                {
                                    var queueActionResult = additionalIpDomain.CreateAdditionalIP(server, stepRequest.Params["networkInterfaceName"]);
                                    if (queueActionResult.Results.ContainsKey("additionalip"))
                                    {
                                        var vlan = server.Resources.NetworkInterfaces.Where(x => x.Name == stepRequest.Params["networkInterfaceName"]).FirstOrDefault().Vlan;
                                        var additionalIp = queueActionResult.Results["additionalip"];
                                        var serverTargetId = Convert.ToInt32(stepRequest.Params["serverTargetId"]);
                                        var serverTarget = _serverDomain.Get(x => x.ServerId == serverTargetId);
                                        if (!string.IsNullOrEmpty(serverTarget.HypervisorIdentifier))
                                            _serverDomain.Load(serverTarget);
                                        
                                        stepResult = additionalIpDomain.CreateVip(serverTarget, additionalIp, serverTarget.Resources.NetworkInterfaces.Where(x => x.Vlan == vlan).FirstOrDefault().Name);
                                    }
                                    break;
                                }
                            case "createip":
                                {
                                    stepResult = additionalIpDomain.CreateAdditionalIP(server, stepRequest.Params["networkInterfaceName"]);
                                    break;
                                }
                        }
                        break;
                    }
                case (int)TypeActionStep.Server_SendInstalServerInstructions:
                    {
                        var sendInstructionsDomain = new SendInstructionsDomain(_context);
                        stepResult = sendInstructionsDomain.SendInstallServerInstructions(server);

                        break;
                    }
                case (int)TypeActionStep.Server_SendChangeConfigurationInstructions:
                    {
                        var sendInstructionsDomain = new SendInstructionsDomain(_context);
                        if (server.StatusId != (int)Status.Deleted)
                        {
                            var operation = stepRequest.Params["operation"];
                            stepResult = sendInstructionsDomain.SendChangeConfigurationServerInstructions(server, operation);
                        }
                        else
                            stepResult = sendInstructionsDomain.SendDeleteServerInstructions(server);
                        break;
                    }

                
              
                case (int)TypeActionStep.Server_CreateNetworkInterface:
                    {
                        var ipServerDomain = new IpServerDomain();
                        var bandWidth = Convert.ToInt32(stepRequest.Params["bandWidth"]);
                        var vlanId = stepRequest.Params.Count > 1 ? Convert.ToInt32(stepRequest.Params["vlanId"]) : 0;
                        var zabbixTemplate = server.Image.Plataform.ToLower().Contains("windows") ? "Template_DOMU_Windows" : "Template_DOMU_Linux";

                        stepResult = ipServerDomain.CreateNetworkinterface(server, bandWidth, vlanId, zabbixTemplate);
                        break;
                    }
                case (int)TypeActionStep.Server_DeleteNetworkInterface:
                    {
                        var ipServerDomain = new IpServerDomain();
                        var mac = string.Empty;

                        if (server.GetHypervisor() == TypeHypervisor.HYPERV_SP1)
                            if (server.Resources.NetworkInterfaces.Where(x => x.Name == stepRequest.Params["networkInterfaceName"]).Count() > 0)
                                mac = server.Resources.NetworkInterfaces.Where(x => x.Name == stepRequest.Params["networkInterfaceName"]).FirstOrDefault().Mac;
                            else
                                mac = stepRequest.Params["networkInterfaceName"];
                        else
                            mac = server.Resources.NetworkInterfaces.Where(x => x.Name == stepRequest.Params["networkInterfaceName"]).FirstOrDefault().Mac;
                        
                        stepResult = ipServerDomain.DeleteNetworkinterface(server, mac);
                        break;
                    }
                case (int)TypeActionStep.Server_ConfigureNetworkInterface:
                    {
                        var ipServerDomain = new IpServerDomain();
                        var bandWidth = Convert.ToInt32(stepRequest.Params["bandWidth"]);
                        var vlanId = stepRequest.Params.Count > 1? Convert.ToInt32(stepRequest.Params["vlanId"]): 0;
                        var zabbixTemplate = server.Image.Plataform.ToLower().Contains("windows") ? "Template_DOMU_Windows" : "Template_DOMU_Linux";

                        stepResult = ipServerDomain.ConfigureNetworkinterface(server, server.Resources.NetworkInterfaces[0].Mac, bandWidth, zabbixTemplate, vlanId);
                        break;
                    }
                default:
                    throw new Exception("erro ao processar step");
            }
            return stepResult;
        }

        private string GetActionUser(QueueActionStep step)
        {
            string user = _clientDomain.Get(x => x.ClientId == step.QueueAction.ClientId) == null
                    ? ""
                    : _clientDomain.Get(x => x.ClientId == step.QueueAction.ClientId).CustomerCode;
          
            return user;
        }

        public void VerifyStepStatus_Server(QueueActionStep step)
        {
            var serverId = step.QueueAction.QueueActionReference.ValueId;

            var server = _serverDomain.Get(x => x.ServerId == serverId);

            IServiceCodeMonitor serviceCodeMonitor = _serviceCodeMonitorFactory.GetInstance(server);

            var resultActionStep = serviceCodeMonitor.Get(step.ServiceCode);


            // se a comunicação com o serviço não falhar (erro:500, 404)
            if (resultActionStep.Status == StatusService.SUCCESS)
            {
                if (resultActionStep.Result.Contains("FAILED") || resultActionStep.Result.Contains("ROLLED_BACK") || resultActionStep.Result.Contains("REJECTED"))
                {
                    step.StatusActionId = (int)StatusQueueAction.FAILED;
                    step.EndDate = DateTime.Now;
                }
                if (resultActionStep.Result.Contains("ON_QUEUE"))
                {
                    //segue a vida !
                }
                if (resultActionStep.Result.Contains("COMPLETED"))
                {
                    if (step.QueueAction.QueueActionReference.TypeQueueActionReferenceId == (int)TypeQueueActionReference.VM_SERVER)
                    {
                        FinalizeServerDetails(resultActionStep, step, server);
                        if (string.IsNullOrEmpty(server.Name))
                            return; // fix: para casos em que api xen/kvm retorna em branco
                    }

                    step.StatusActionId = (int)StatusQueueAction.COMPLETED;
                }
            }
            else
            {
                step.StatusActionId = (int)StatusQueueAction.FAILED;

                // caso erro de comunicação, não se altera o status do step até ter uma resposta.
                _logger.Log(
                    string.Format(
                        "{3} : Erro de comunição com hypervisor. QueueActionId: {0}, QueueActionStepId: {1} CustomerCode {2}",
                        step.QueueActionId, step.QueueActionStepId, server.Client.CustomerCode, LogUtils.GetCurrentMethod(this)), true);
            }


            step.ResultBody = resultActionStep.Result;
            step.LastUpdate = DateTime.Now;
        }

        private void FinalizeServerDetails(ServiceResult resultActionStep, QueueActionStep step, Server server)
        {
            StepRequest stepRequest = new StepRequest().Deserialize(step.RequestBody);

            if (step.TypeActionStepId == (int)TypeActionStep.Server_InstallServer)
            {
                server.StatusId = (int)Status.Active;
                _serverDomain.AddUpdateServer(server);
            }

            if (step.TypeActionStepId == (int)TypeActionStep.Server_UninstallServer)
            {
                server.StatusId = (int)Status.Deleted;
                server.EndDate = DateTime.Now;
                _serverDomain.AddUpdateServer(server);
                return;
            }

            if (step.TypeActionStepId == (int)TypeActionStep.Server_SupendServer)
            {
                server.StatusId = (int)Status.Suspended;
                _serverDomain.AddUpdateServer(server);
                return;
            }

            if (step.TypeActionStepId == (int)TypeActionStep.Server_CreateClone)
            {
                server.TypeInstallOriginId = (int)TypeInstallOrigin.Clone;
                _serverDomain.AddUpdateServer(server);
            }

            if (step.TypeActionStepId == (int)TypeActionStep.Server_ReactivateServer)
            {
                server.StatusId = (int)Status.Active;
                _serverDomain.AddUpdateServer(server);
            }

            if (step.TypeActionStepId == (int)TypeActionStep.Server_ReinstallServer)
            {
                if (Convert.ToInt32(stepRequest.Params["typeImageId"]) == (int)TypeImage.Private)
                {
                    server.ImageId = new ImageDomain(new EngineCloudDataContext()).Get(x => x.Name == "imagem-privada").ImageId;
                }
                else
                {
                    server.ImageId = Convert.ToInt32(stepRequest.Params["imageId"]);
                }

                _serverDomain.AddUpdateServer(server);

                return;
            }

            if (step.TypeActionStepId == (int)TypeActionStep.Server_DeleteDisk)
                return;


            if (resultActionStep.Results.Any())
            {
                // atualiza com aquilo que pode mudar em termos de BD.
                if (resultActionStep.Results.ContainsKey("serverName") &&
                    !string.IsNullOrEmpty(resultActionStep.Results["serverName"]))
                    server.Name = resultActionStep.Results["serverName"];

                if (resultActionStep.Results.ContainsKey("hypervisorIdentifier") &&
                    !string.IsNullOrEmpty(resultActionStep.Results["hypervisorIdentifier"]))
                    server.HypervisorIdentifier = resultActionStep.Results["hypervisorIdentifier"];

                if (resultActionStep.Results.ContainsKey("ip") && !string.IsNullOrEmpty(resultActionStep.Results["ip"]))
                    server.IP = resultActionStep.Results["ip"];

                _serverDomain.AddUpdateServer(server);
            }
        }
    }
}