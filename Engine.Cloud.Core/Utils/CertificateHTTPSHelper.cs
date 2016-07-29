using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Engine.Cloud.Core.Utils
{
    public static class CertificateHTTPSHelper
    {
        public static void ByPassCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
    }
}
