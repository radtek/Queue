using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public class ServiceCodeMonitorHyperV : HyperVServiceBase, IServiceCodeMonitor
    {
        public ServiceResult Get(string serviceCode)
        {
            try
            {
                //var jobs2 = base.ContextVmm.Jobs.Where(x => x.TargetObjectID == Guid.Parse("269390c0-0a18-4007-848c-b5bfc4538671"));

                var jobs = base.ContextVmm.Jobs.Where(x => x.ID == Guid.Parse(serviceCode));

                Throw.IfIsTrue(jobs.Count() == 0, new Exception(string.Format("VMMJobID {0} not found", serviceCode)));

                if (jobs.First().StartTime.HasValue && jobs.First().ProgressValue == 0)
                {
                    var startTime = jobs.First().StartTime.Value;
                    var timeTimeSpan = DateTime.Now.Subtract(startTime);

                    if (timeTimeSpan.TotalMinutes > 30)
                        return new ServiceResult() { Result = "FAILED", Status = StatusService.SUCCESS };
                }

                if (jobs.First().IsCompleted == false)
                    return new ServiceResult() { Result = "ON_QUEUE", Status = StatusService.SUCCESS };
                
                if (jobs.First().Status.ToLower().Contains("failed"))
                    return new ServiceResult() { Result = "FAILED", Status = StatusService.SUCCESS };

                Dictionary<string, string> results = new Dictionary<string, string>();

                if (jobs.First().ResultObjectType != null && jobs.First().ResultObjectType.ToUpper() == "VM")
                {
                    var resultName = jobs.First().ResultName;

                    var virtualMachine = base.ContextVmm.VirtualMachines.Where(x => x.Name == resultName).First();

                    results = new Dictionary<string, string>();
                    results.Add("serverName", resultName);
                    results.Add("hypervisorIdentifier", virtualMachine.ID.ToString());
                    results.Add("result", "COMPLETED");
                }

                return new ServiceResult() { Result = "COMPLETED", Status = StatusService.SUCCESS, Results = results };

            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return new ServiceResult() { Result = ex.Message, Status = StatusService.FAILED };
            }
        }
    }
}
