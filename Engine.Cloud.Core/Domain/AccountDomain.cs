using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;
using Utils;

namespace Engine.Cloud.Core.Domain
{
    public class AccountDomain
    {
        private readonly string _url;


        private readonly EngineCloudDataContext _context;

        public AccountDomain(EngineCloudDataContext context)
        {
            _context = context;
            _url = AppSettings.GetString("ServiceSupport.UrlWS");
        }

        public Client Get(Expression<Func<Client, bool>> predicate)
        {
            return _context.Client.FirstOrDefault(predicate);
        }

        public string GetProfile(string customerCode)
        {
            var url = _url + "/customer/profile/" + customerCode;

            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpResponseMessage = httpClient.GetAsync(url).Result;
                if (!httpResponseMessage.IsSuccessStatusCode)
                    return null;
            }

            var result = httpResponseMessage.Content.ReadAsStringAsync().Result;

            return result;
        }
    }
}

