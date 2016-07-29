using Engine.Cloud.Core.Model;
using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public class IPServerServiceKvmSp : IPServerServiceXenSp
    {
        public IPServerServiceKvmSp(Server server): base(server)
        {
            UrlBase = string.Format("{0}kratos/rest/", ConfigurationManager.AppSettings.Get("Cloud.KvmSP1.UrlWS"));
        }
    }
}