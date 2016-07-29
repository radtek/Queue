using System;
using System.Collections.Generic;

namespace Engine.Cloud.Core.Domain.Services.QueueAction
{
    public abstract class ActionRequest
    {
        public virtual int ClientId { get; set; }
        
        public int TypeActionId { get; set; }
        public string Name { get; set; }
        public string User { get; set; }
        public long AfterQueueActionId { get; set; }
        public string IpAddress { get; set; }
        public int TotalCompleted { get; set; }
        public string Result { get; set; }
        public DateTime? EndDate{ get; set; }

        public List<StepRequest> Steps = new List<StepRequest>();

    }

    public class ActionClientRequest : ActionRequest
    {
        public override int ClientId { get; set; }
    }

    public class ActionServerRequest : ActionRequest
    {
        public long ServerId { get; set; }
    }

    public class ActionBackupRequest : ActionRequest
    {
        public long BackupId { get; set; }
    }

    public class ActionLoadBalanceRequest : ActionRequest
    {
        public long LoadBalanceId { get; set; }
    }

    public class ActionPrivateImageRequest : ActionRequest
    {
        public long PrivateImageId { get; set; }
    }
    public class ActionMailManagerRequest : ActionRequest
    {
        public long MailManagerId { get; set; }
    }

    public class ActionPrivateVlanRequest : ActionRequest
    {
        public long PrivateVlanId { get; set; }
    }
}