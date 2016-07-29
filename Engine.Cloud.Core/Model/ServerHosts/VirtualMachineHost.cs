using Newtonsoft.Json;
using System.Collections.Generic;

namespace Engine.Cloud.Core.Model.ServerHosts
{
    public class VirtualMachineHost
    {
        public VirtualMachineHost()
        {
        }

        public Collection collection { get; set; }

        public class Collection
        {
            public Collection()
            {
                host = new List<Host>();
            }

            public List<Host> host { get; set; }

            public class Host
            {
                public string flags { get; set; }
                public string identifier { get; set; }
                public string name { get; set; }
                public string region { get; set; }
                public string admState { get; set; }
                public string ip { get; set; }
                public string keepalive { get; set; }
                public string opState { get; set; }
                public string port { get; set; }
                public string technology { get; set; }
                public string version { get; set; }
                public string freeMemory { get; set; }
                public string frequency { get; set; }
                public string loadAverage { get; set; }
                public string memory { get; set; }
                public string state { get; set; }

                public StorageObject storages { get; set; }

                public class StorageObject
                {
                    public StorageObject()
                    {
                        storage = new List<StorageItem>();
                    }

                    public List<StorageItem> storage { get; set; }

                    public class StorageItem
                    {
                        public string identifier { get; set; }
                        public string name { get; set; }
                        public string region { get; set; }
                        public string state { get; set; }
                        public string admState { get; set; }
                        public string ip { get; set; }
                        public string keepalive { get; set; }
                        public string opState { get; set; }
                        public string port { get; set; }
                        public string technology { get; set; }
                        public string version { get; set; }
                        public string freeSize { get; set; }
                        public string internalIp { get; set; }
                        public string size { get; set; }
                    }
                }
            }
        }
    }
}