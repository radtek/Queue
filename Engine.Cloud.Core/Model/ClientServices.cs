using Newtonsoft.Json;
using System.Collections.Generic;

namespace Engine.Cloud.Core.Model
{
    [JsonObject]
    public class ClientServices
    {

        public ClientServices()
        {
            ListProduct = new List<Product>();
        }

        public List<Product> ListProduct { get; set; }
        public long ClientId { get; set; }
        public string Client { get; set; }
        public string CpfCnpj { get; set; }

        public class Product
        {
            public int Id { get; set; }
            public string Initials { get; set; }
            public string Label { get; set; }
            public int Orders { get; set; }
        }
    }
}
