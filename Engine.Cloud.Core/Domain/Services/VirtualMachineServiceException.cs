using System;

namespace Engine.Cloud.Core.Domain.Services
{
    public class VirtualMachineServiceException : Exception
    {
        public VirtualMachineServiceException(string message)
            : base(message)
        {

        }

        public VirtualMachineServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}