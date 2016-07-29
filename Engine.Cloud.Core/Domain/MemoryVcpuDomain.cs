using Engine.Cloud.Core.Domain.Services.MemoryVcpu;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Extensions;
using System;
using Engine.Cloud.Core.Utils.Logging;
using Utils;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain
{
    public class MemoryVcpuDomain
    {
        private readonly MemoryVcpuServiceFactory _memoryVcpuFactory;
        private ServerDomain _serverDomain;
        private EngineCloudDataContext _context;
        private readonly ILogger _logger;

        public MemoryVcpuDomain(EngineCloudDataContext context)
        {
            _context = context;
            _memoryVcpuFactory = new MemoryVcpuServiceFactory();
            _logger = LogFactory.GetInstance();
        }

        public QueueActionResult Change(Server server, byte vcpu, int frequency, int memory)
        {
            try
            {
                var memoryVcpuService = _memoryVcpuFactory.GetInstance(server);
                var serviceResult = memoryVcpuService.Change(vcpu, frequency, memory);

                server.StatusBlockId = (int)StatusBlock.Busy;
                server.MessageStatus = TypeActionStep.Server_ChangeMemoryVcpu.GetDescription();
                
                _serverDomain = _context != null ? new ServerDomain(_context) : new ServerDomain(new EngineCloudDataContext());
                _serverDomain.AddUpdateServer(server);

                return new QueueActionResult { Result = serviceResult.Result, Status = StatusQueueAction.PENDING };
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }
    }
}
