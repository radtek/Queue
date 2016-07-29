using System;
using Engine.Cloud.Core.Domain.Services.AdditionalIP;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.Core.Domain
{
    public class IpServerDomain
    {
        private readonly IPServiceFactory _serviceFactory;
        private readonly ILogger _logger;


        public IpServerDomain()
        {
            _serviceFactory = new IPServiceFactory();
            _logger = LogFactory.GetInstance();
        }

        public QueueActionResult CreateAdditionalIP(Server server, string networkInterfaceName)
        {
            try
            {
                var additionalIpService = _serviceFactory.GetInstance(server);
                var result = additionalIpService.CreateAdditionalIP(networkInterfaceName);

                return result.Status == StatusService.SUCCESS
                    ? new QueueActionResult { Result = result.Result, Status = StatusQueueAction.PENDING , Results = result.Results}
                    : new QueueActionResult { Result = result.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult DeleteAdditionalIP(Server server, string ip)
        {
            try
            {
                var additionalIpService = _serviceFactory.GetInstance(server);
                var result = additionalIpService.DeleteAdditionalIP(ip);

                return result.Status == StatusService.SUCCESS
                    ? new QueueActionResult { Result = result.Result, Status = StatusQueueAction.PENDING }
                    : new QueueActionResult { Result = result.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult CreateVip(Server server, string newIp, string networkInterfaceName)
        {
            try
            {
                var additionalIpService = _serviceFactory.GetInstance(server);
                var result = additionalIpService.CreateVip(server, newIp, networkInterfaceName);

                return result.Status == StatusService.SUCCESS
                    ? new QueueActionResult { Result = result.Result, Status = StatusQueueAction.PENDING }
                    : new QueueActionResult { Result = result.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

        }

        public QueueActionResult CreateNetworkinterface(Server server, int bandwidth, int vlanId, string zabbixTemplate)
        {
            try
            {
                var ipServerService = _serviceFactory.GetInstance(server);
                var result = ipServerService.CreateNetworkinterface(bandwidth, vlanId, zabbixTemplate);

                return result.Status == StatusService.SUCCESS
                    ? new QueueActionResult { Result = result.Result, Status = StatusQueueAction.COMPLETED }
                    : new QueueActionResult { Result = result.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult DeleteNetworkinterface(Server server, string macAddress)
        {
            try
            {
                var ipServerService = _serviceFactory.GetInstance(server);
                var result = ipServerService.DeleteNetworkinterface(macAddress);

                return result.Status == StatusService.SUCCESS
                    ? new QueueActionResult { Result = result.Result, Status = StatusQueueAction.COMPLETED }
                    : new QueueActionResult { Result = result.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult ConfigureNetworkinterface(Server server, string macAddress, int bandwidth, string zabbixTemplate, int vlanId)
        {
            try
            {
                var ipServerService = _serviceFactory.GetInstance(server);
                var result = ipServerService.ConfigureNetworkinterface(macAddress, bandwidth,  zabbixTemplate, vlanId);

                return result.Status == StatusService.SUCCESS
                    ? new QueueActionResult { Result = result.Result, Status = StatusQueueAction.COMPLETED }
                    : new QueueActionResult { Result = result.Result, Status = StatusQueueAction.FAILED };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        public QueueActionResult UpdateNetworkInterface(Server server, int bandwidth)
        {
            throw new NotImplementedException();
        }
    }
}
