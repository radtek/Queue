using Engine.Cloud.Core.Model;
using System.Configuration;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public class IPServerServiceXenRj : IPServerServiceXenSp
    {
        public IPServerServiceXenRj(Server server) : base(server)
        {
            UrlBase = string.Format("{0}cloud/rest/", ConfigurationManager.AppSettings.Get("Cloud.XenRJ1.UrlWS"));
        }
    }
}