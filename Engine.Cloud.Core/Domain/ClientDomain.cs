using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils;

namespace Engine.Cloud.Core.Domain
{
    public class ClientDomain
    {
        private string _url;
        private readonly EngineCloudDataContext _context;


        public ClientDomain(EngineCloudDataContext context)
        {
            _context = context;
            _url = AppSettings.GetString("ServiceSupport.UrlWS");
        }

        public Client Get(Expression<Func<Client, bool>> predicate)
        {
            return _context.Client.Where(predicate).FirstOrDefault();
        }

        public IEnumerable<Client> GetAll(Expression<Func<Client, bool>> predicate)
        {
            return _context.Client.Where(predicate);
        }

        public Client AddUpdateClient(Client client)
        {
            _context.Client.Attach(client);

            _context.Entry(client).State = client.ClientId == 0
                ? EntityState.Added
                : EntityState.Modified;

            _context.SaveChanges();

            return client;
        }

        public string GetEmailCustomer(string customerCode)
        {
            var result = GetProfille(customerCode);

            Throw.IfIsNull(result);

            var jsonDictinary = DeserializeToDictionary(result);

            if (jsonDictinary.ContainsKey("Email"))
            {
                var value = jsonDictinary["Email"];
                return value.ToString();
            }
            return null;
        }


        private static Dictionary<string, object> DeserializeToDictionary(string json)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var result = new Dictionary<string, object>();
            foreach (var item in values)
            {
                if (item.Value is JObject)
                {
                    result.Add(item.Key, DeserializeToDictionary(item.Value.ToString()));
                }
                else
                {
                    result.Add(item.Key, item.Value);
                }
            }
            return result;
        }

        private string GetProfille(string customerCode)
        {
            var url = _url + "/customer/profile/" + customerCode;

            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpResponseMessage = httpClient.GetAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            var result = httpResponseMessage.Content.ReadAsStringAsync().Result;

            
            return result;
        }
    }
}