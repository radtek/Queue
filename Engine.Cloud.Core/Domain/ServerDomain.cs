using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Engine.Cloud.Core.Domain.Services.VirtualMachine;
using Engine.Cloud.Core.Model.DataContext;
using Engine.Cloud.Core.Model.Requests;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Logging;
using Utils;
using Engine.Cloud.Core.Model;

namespace Engine.Cloud.Core.Domain
{
    public class ServerDomain
    {
        private readonly EngineCloudDataContext _context;
        private readonly VirtualMachineServiceFactory _factory;
        private readonly ClientDomain _clientDomain;

        private readonly ImageDomain _imageDomain;
        private readonly ILogger _logger;

        public ServerDomain(EngineCloudDataContext context)
        {
            _context = context;
            _clientDomain = new ClientDomain(context);
            _imageDomain = new ImageDomain(context);
            _factory = new VirtualMachineServiceFactory();
            _logger = LogFactory.GetInstance();
        }

        public Server Get(Expression<Func<Server, bool>> predicate)
        {
            return _context.Server.Where(predicate).OrderByDescending(x => x.LastUpdateDate).FirstOrDefault();
        }

        public IEnumerable<Server> GetAll(Expression<Func<Server, bool>> predicate)
        {
            return _context.Server.Where(predicate);
        }

        public void Load(Server server)
        {
            try
            {
                server.Image = _imageDomain.Get(x => x.ImageId == server.ImageId);
                var virtualMachineService = _factory.GetInstance(server);

                if (AppSettings.GetBoolean("Cloud.Servers.BlockAll"))
                {
                    server.StatusBlockId = 1;
                    return;
                }

                if (server.Name.IsValidGuid())
                    server.RemoteStatus = RemoteStatus.DontExist;
                else
                {
                    virtualMachineService.Load(server);
                    TrySyncRemoteIP(server);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                throw;
            }
        }

        public Server Load(TypeHypervisor typeHypervisor, string hypervisorIdentifier) // todo : usar  Load(Server server). isso eh usado apenas na auditoria.
        {
            try
            {
                var virtualMachineService = _factory.GetInstance(typeHypervisor);
                return virtualMachineService.Load(hypervisorIdentifier);
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                throw;
            }
        }

        public void LoadAll(List<Server> servers, TypeHypervisor typeHypervisor)
        {
            try
            {
                var virtualMachineService = _factory.GetInstance(typeHypervisor);
                virtualMachineService.LoadAll(servers);
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                throw;
            }
        }

        public Server AddUpdateServer(Server server)
        {
            server.Image = _context.Image.First(x => x.ImageId == server.ImageId);
            server.Plan = _context.Plan.First(x => x.PlanId == server.PlanId);
            server.Client = _context.Client.First(x => x.ClientId == server.ClientId);

            _context.Server.Attach(server);

            _context.Entry(server).State = server.ServerId == 0
                ? EntityState.Added
                : EntityState.Modified;

            server.LastUpdateDate = DateTime.Now;
            if (_context.Entry(server).State == EntityState.Added)
                server.CreateDate = DateTime.Now;

            _context.SaveChanges();

            return server;
        }

        public QueueActionResult Install(InstallServerRequest installServerRequest)
        {
            try
            {
                var client = _clientDomain.Get(x => x.CustomerCode == installServerRequest.CustomerCode);
                var server = this.Get(x => x.ServerId == installServerRequest.ServerId);

                Image image = _imageDomain.Get(x => x.ImageId == installServerRequest.ImageId);

                var hipervisorService = _factory.GetInstance(image);

                var serviceResult = hipervisorService.Install(client, installServerRequest.Vcpu,
                    installServerRequest.Frequency,
                    installServerRequest.Memory, image.ImageId.ToString(), installServerRequest.Disk,
                    installServerRequest.BandWidths, installServerRequest.Partitions, installServerRequest.FormatDisk, server.ServerId);

                if (serviceResult.Status == StatusService.SUCCESS)
                {
                    server.HypervisorIdentifier = serviceResult.Result;
                    this.AddUpdateServer(server);
                }

                return serviceResult.Status == StatusService.SUCCESS
                    ? new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING }
                    : new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Uninstall(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);

                if (server.RemoteStatus == RemoteStatus.DontExist)
                {
                    server.StatusId = (int)Status.Deleted;
                    server.EndDate = DateTime.Now;
                    AddUpdateServer(server);

                    _logger.Log(string.Format("Remoção de servidor [{0}] não realizada no hypervisor. Servidor já não existe.", server.Name));
                    return new QueueActionResult() { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
                }

                var serviceResult = hipervisorService.Uninstall(server);

                return serviceResult.Status == StatusService.SUCCESS
                    ? new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING }
                    : new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Reinstall(Server server, int imageId, int typeImageId)
        {
            try
            {
                var image = _imageDomain.Get(x => x.ImageId == imageId);

                var hipervisorService = _factory.GetInstance(image);

                var serviceResult = hipervisorService.Reinstall(server, image);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Suspend(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Suspend(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Suspendendo";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Reactivate(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Unlock(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.COMPLETED };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Reboot(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Reboot(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Reiniciando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Resume(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Resume(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Reativando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Reset(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Reset(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Reiniciando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Pause(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Pause(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Pausando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult PowerOn(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.PowerOn(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Ligando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult PowerOff(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.PowerOff(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Desligando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult ShutDown(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.ShutDown(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Desligando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult Refresh(Server server)
        {
            try
            {
                var hipervisorService = _factory.GetInstance(server);
                var serviceResult = hipervisorService.Refresh(server);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = "Atualizando";
                AddUpdateServer(server);

                if (serviceResult.Status != StatusService.FAILED)
                    return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
                return new QueueActionResult() { Result = serviceResult.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public void TrySyncRemoteIP(Server server)
        {
            try
            {
                if (server.RemoteStatus == RemoteStatus.DontExist)
                    return;

                var networkInterfaces = server.Resources.NetworkInterfaces.FirstOrDefault();

                var ips = networkInterfaces?.Ips.FirstOrDefault();

                if (ips == null) return;

                if (server.IP == ips.Number) return;

                server.IP = ips.Number;
                AddUpdateServer(server);
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0} - {1}", LogUtils.GetCurrentMethod(this), server.Name), ex);
            }
        }


    }
}
