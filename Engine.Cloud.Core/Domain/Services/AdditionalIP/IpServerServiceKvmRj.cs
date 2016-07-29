using Engine.Cloud.Core.Model;
using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public class IpServerServiceKvmRj : IPServerServiceKvmSp
    {
        public IpServerServiceKvmRj(Server server) : base(server)
        {
            UrlBase = string.Format("{0}kratos/rest/", ConfigurationManager.AppSettings.Get("Cloud.KvmRJ1.UrlWS"));
        }
    }
}