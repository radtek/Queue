using System;
using System.Linq;
using Engine.Cloud.Core.Utils;
using Utils;

namespace Engine.Cloud.Core.Domain.Services
{
    public class HyperVServiceBase
    {
        protected ServiceReferenceVmm.VMM ContextVmm;
        protected ServiceReferenceAdmin.Admin ContextAdmin;
        protected ServiceReferenceAdmin.Stamp StampVmm;

        protected ServiceReferenceVmm.Cloud CloudOnVmm;
        protected string UrlApiVmm;
        protected ILogger _logger;

        public HyperVServiceBase()
        {
            _logger = LogFactory.GetInstance();
            UrlApiVmm = AppSettings.GetString("Cloud.HYPERV_SP1.Vmm.UrlAPI");

            string urlSpfVmm = AppSettings.GetString("Cloud.HYPERV_SP1.Vmm.UrlWS");
            string urlSpfAdmin = AppSettings.GetString("Cloud.HYPERV_SP1.Admin.UrlWS");

            string vmmServer = AppSettings.GetString("Cloud.HYPERV_SP1.VmmServer");
            string vmmCloudName = AppSettings.GetString("Cloud.HYPERV_SP1.VmmCloudName");

            string user = AppSettings.GetString("Cloud.HYPERV_SP1.user");
            string password = AppSettings.GetString("Cloud.HYPERV_SP1.password");

            CertificateHTTPSHelper.ByPassCertificate();

            ContextVmm = new ServiceReferenceVmm.VMM(new Uri(urlSpfVmm));
            ContextVmm.Credentials = new System.Net.NetworkCredential(user, password);

            ContextAdmin = new ServiceReferenceAdmin.Admin(new Uri(urlSpfAdmin));
            ContextAdmin.Credentials = new System.Net.NetworkCredential(user, password);

            StampVmm = ContextAdmin.Stamps.Where(x => x.Name == vmmServer).First(); // VMM da vez
            CloudOnVmm = ContextVmm.Clouds.Where(x => x.Name == vmmCloudName).First(); // Cloud da vez
        }
    }
}