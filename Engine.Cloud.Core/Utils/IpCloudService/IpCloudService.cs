using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.Core.Utils.IpCloudService
{

    public class IPCloudService
    {
        private readonly List<string> _urlList = new List<string>();
        private readonly ILogger _logger;

        public IPCloudService()
        {
            _urlList.Add(string.Format("{0}cloud/rest/virtualmachine/ip/", AppSettings.GetString("Cloud.XenSP1.UrlWS")));
            _urlList.Add(string.Format("{0}cloud/rest/virtualmachine/ip/", AppSettings.GetString("Cloud.XenRJ1.UrlWS")));
            _urlList.Add(string.Format("{0}kratos/rest/virtualmachine/ip/", AppSettings.GetString("Cloud.KvmSP1.UrlWS")));

            _logger = LogFactory.GetInstance();
        }

        public string GetServerName(string ip)
        {
            var result = string.Empty;

            Parallel.ForEach(_urlList, url =>
            {
                var response = MakeRequest(url + ip);

                if (string.IsNullOrEmpty(result))
                {
                    result = response;
                }
            });

            return result;
        }

        private string MakeRequest(string url)
        {
            try
            {
                var xmlDocument = new XmlDocument();

                HttpResponseMessage httpResponseMessage;

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(url);
                    httpResponseMessage = httpClient.GetAsync(url).Result;
                    httpResponseMessage.EnsureSuccessStatusCode();
                }

                xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);

                if (xmlDocument.SelectSingleNode("virtualmachine/state").InnerText.Contains("DONT_EXIST"))
                    return string.Empty;

                var result = string.Empty;
                if (xmlDocument.SelectSingleNode("virtualmachine/name") != null)
                    result = xmlDocument.SelectSingleNode("virtualmachine/name").InnerText;

                return result;
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}, url: {1}", LogUtils.GetCurrentMethod(this), url), ex);
                throw;
            }
        }
    }
}
