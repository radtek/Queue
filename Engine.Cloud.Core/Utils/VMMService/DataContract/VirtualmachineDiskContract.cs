using System.Collections.Generic;

namespace Engine.Cloud.Core.Utils.VMMService.DataContract
{
    public class VirtualmachineDiskContract
    {
        public List<Item> Disk { get; set; }

        public VirtualmachineDiskContract()
        {
            Disk = new List<Item>();
        }
        
        public class Item
        {
            public string Name { get; set; }
        }
    }
}
