using System;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Extensions;
using Engine.Cloud.Core.Utils.Logging;
using Polly;
using Utils;
using Engine.Cloud.Core.Model.DataContext;

namespace Engine.Cloud.Core.Domain.Services.QueueAction
{
    public class QueueActionService
    {
        private readonly EngineCloudDataContext _context;

        private readonly QueueActionDomain _queueActionDomain;
        private readonly QueueActionStepDomain _queueActionStepDomain;
        private readonly ILogger _logger;

        private static readonly object _lock = new object();

        public QueueActionService(EngineCloudDataContext context)
        {
            _context = context;
            _queueActionDomain = new QueueActionDomain(context);
            _queueActionStepDomain = new QueueActionStepDomain(context);
            _logger = LogFactory.GetInstance();
        }

        public long SaveOperation(ActionRequest actionRequest)
        {
            var queueActionReference = new QueueActionReference();

            if (actionRequest is ActionClientRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.CLIENT;
                queueActionReference.ValueId = ((ActionClientRequest)actionRequest).ClientId;
            }
            else if (actionRequest is ActionServerRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.VM_SERVER;
                queueActionReference.ValueId = ((ActionServerRequest)actionRequest).ServerId;
            }
            else if (actionRequest is ActionBackupRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.BACKUP;
                queueActionReference.ValueId = ((ActionBackupRequest)actionRequest).BackupId;
            }
            else if (actionRequest is ActionLoadBalanceRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.LOADBALANCE;
                queueActionReference.ValueId = ((ActionLoadBalanceRequest)actionRequest).LoadBalanceId;
            }
            else if (actionRequest is ActionPrivateImageRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.PRIVATE_IMAGE;
                queueActionReference.ValueId = ((ActionPrivateImageRequest)actionRequest).PrivateImageId;
            }
            else if (actionRequest is ActionMailManagerRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.MAILMANAGER;
                queueActionReference.ValueId = ((ActionMailManagerRequest)actionRequest).MailManagerId;
            }
            else if (actionRequest is ActionPrivateVlanRequest)
            {
                queueActionReference.TypeQueueActionReferenceId = (int)TypeQueueActionReference.PRIVATEVLANMANAGER;
                queueActionReference.ValueId = ((ActionPrivateVlanRequest)actionRequest).PrivateVlanId;
            }
            else
                throw new Exception("queueactionreference inválido");


            var action = new Model.QueueAction();
            action.QueueActionReference = (queueActionReference);

            action.TypeActionId = actionRequest.TypeActionId;
            action.StatusActionId = actionRequest.EndDate != null ? (int)StatusQueueAction.COMPLETED : (int)StatusQueueAction.ON_QUEUE;
            action.Name = actionRequest.Name;
            action.User = actionRequest.User;
            action.ClientId = actionRequest.ClientId;
            action.IpAddress = actionRequest.IpAddress;
            action.TotalCompleted = actionRequest.TotalCompleted;
            action.AfterQueueActionId = actionRequest.AfterQueueActionId > 0 ? (long?)actionRequest.AfterQueueActionId : null;

            action.CreateDate = DateTime.Now;
            action.EndDate = actionRequest.EndDate ?? actionRequest.EndDate;
            action.Result = actionRequest.Result != "" ? actionRequest.Result : null;

            foreach (var step in actionRequest.Steps)
            {
                var actionStep = new QueueActionStep();
                actionStep.RequestBody = step.Serialize();
                actionStep.Name = step.Name;
                actionStep.TypeActionStepId = step.TypeActionStepId;
                actionStep.CreateDate = DateTime.Now;

                action.QueueActionStep.Add(actionStep);
            }

            _queueActionDomain.CreateQueueAction(action);

            return action.QueueActionId;
        }

        public void SendAuditAction(string description, int clientId, string userDetail, string ipAddress)
        {

            this.SaveOperation(new ActionClientRequest
            {
                ClientId = clientId,
                User = userDetail,
                IpAddress = ipAddress,
                Name = description,
                TypeActionId = (int)TypeAction.AuditActionPanel,
                TotalCompleted = 100,
                Result = StatusQueueAction.COMPLETED.GetDescription(),
                EndDate = DateTime.Now
            });
        }

        public QueueActionResult ExecuteStep(QueueActionStep step)
        {
            try
            {
                QueueActionResult stepResult;

                var typeActionStep = Enum.GetName(typeof(TypeActionStep), step.TypeActionStepId);

                if (typeActionStep == null)
                    return new QueueActionResult();

                if (typeActionStep.ToLower().StartsWith("server_"))
                    stepResult = new ServerEngine(_context).ExecuteStep(step);
                //else if (typeActionStep.ToLower().StartsWith("backup_"))
                //    stepResult = new BackupEngine(_context).ExecuteStep(step);
                //else if (typeActionStep.ToLower().StartsWith("loadbalance_"))
                //    stepResult = new LoadBalanceEngine(_context).ExecuteStep(step);
                //else if (typeActionStep.ToLower().StartsWith("mailmanager_"))
                //    stepResult = new MailManagerEngine(_context).ExecuteStep(step);
                //else if (typeActionStep.ToLower().StartsWith("privatevlan_"))
                //    stepResult = new PrivateVlanEngine(_context).ExecuteStep(step);
                //else if (typeActionStep.ToLower().StartsWith("privateimage_"))
                //    stepResult = new PrivateImageEngine(_context).ExecuteStep(step);
                //else if (typeActionStep.ToLower().StartsWith("mailmanager_"))
                //    stepResult = new MailManagerEngine(_context).ExecuteStep(step);
                else
                    return new QueueActionResult();

                step.StatusActionId = (int)stepResult.Status;
                step.ResultBody = stepResult.Result;

                if (stepResult.Status == StatusQueueAction.PENDING || stepResult.Status == StatusQueueAction.COMPLETED)
                    step.ServiceCode = stepResult.Result;

                if (stepResult.Status == StatusQueueAction.FAILED || stepResult.Status == StatusQueueAction.ROLLED_BACK)
                    step.ServiceCode = string.Empty;
                
                TryUpdateStep(step);

                return stepResult;
            }
            catch (Exception ex)
            {
                step.StatusActionId = (int)StatusQueueAction.FAILED;
                step.ResultBody = ex.Message;
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);

                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
        }

        private void TryUpdateStep(QueueActionStep step)
        {
            lock (_lock)
            {
                const int LIMIT = 3;

                Policy
                    .Handle<Exception>()
                    .WaitAndRetry(LIMIT, x => TimeSpan.FromSeconds(Math.Pow(2, x)))
                    .Execute(
                        () =>
                            _queueActionStepDomain.AddUpdateQueueActionStep(step));
            }
        }

        public void CheckStepPending(QueueActionStep step)
        {
            var typeActionStep = Enum.GetName(typeof(TypeActionStep), step.TypeActionStepId);

            if (typeActionStep.ToLower().StartsWith("server_"))
                new ServerEngine(_context).VerifyStepStatus_Server(step);
            //else if (typeActionStep.ToLower().StartsWith("privateimage_"))
            //    new PrivateImageEngine(_context).VerifyStepStatus_PrivateImage(step);
            //else if (typeActionStep.ToLower().StartsWith("privatevlan_"))
            //    new PrivateVlanEngine(_context).VerifyStepStatus_PrivateVlan(step);
            else
                throw new Exception("não foi possivel definir tipo de ação");
        }
    }
}
