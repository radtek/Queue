using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using Engine.Cloud.Core.Utils.Logging;
using Engine.Cloud.Core.Utils.VMMService.DataContract;
using Newtonsoft.Json;
using Utils;

namespace Engine.Cloud.Core.Utils.VMMService
{
    public class VMMClient
    {
        private readonly int HTTP_TIMEOUT = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Cloud.HTTP.TIMEOUT"));
        private static readonly string UrlApiVmm = AppSettings.GetString("Cloud.HYPERV_SP1.Vmm.UrlAPI");

        private readonly ILogger _logger;

        public VMMClient()
        {
            _logger = LogFactory.GetInstance();
        }

        public VirtualmachineContract GetVirtualmachine(string id)
        {
            var url = string.Format("{0}/api/virtualmachine/get/{1}", UrlApiVmm, id);

            VirtualmachineContract virtualmachineContract;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                var httpResponseMessage = httpClient.GetAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();

                virtualmachineContract = JsonConvert.DeserializeObject<VirtualmachineContract>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            }

            return virtualmachineContract;
        }

        public VirtualmachineNetworkContract GetNetworkinterface(string id, string macAddress)
        {
            var url = string.Format("{0}/api/virtualmachine/{1}/networkinterface?macAddress={2}", UrlApiVmm, id, macAddress);

            VirtualmachineNetworkContract virtualmachineNetworkContract;
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.GetAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();

                virtualmachineNetworkContract = ParseContract(httpResponseMessage.Content.ReadAsStringAsync().Result, macAddress);
            }

            return virtualmachineNetworkContract;
        }

        private VirtualmachineNetworkContract ParseContract(string httpResponseMessage, string macAddress)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<List<VirtualmachineNetworkContract>>(httpResponseMessage);
                return result.First();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new VirtualmachineNetworkContract() { MacAddress = macAddress};
            }
        }

        public void NetworkinterfaceCreate(string id, string serverName, string customerCode, decimal bandwidth, int vlanId, string zabbixTemplate)
        {
            var url = string.Empty;
            try
            {
                url = string.Format("{0}/api/virtualmachine/{1}/networkinterface?customerCode={2}&bandWidth={3}&zabbixTemplate={4}&serverName={5}{6}", UrlApiVmm, id, customerCode, bandwidth, zabbixTemplate, serverName, vlanId > 0 ? "&vlanId=" + vlanId : "");
                var putData = new List<KeyValuePair<string, string>>();

                _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, putData));
                using (var httpClient = new HttpClient())
                {
                    HttpContent content = new FormUrlEncodedContent(putData);
                    httpClient.BaseAddress = new Uri(url);
                    httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                    HttpResponseMessage httpResponseMessage = httpClient.PostAsync(url, content).Result;
                    httpResponseMessage.EnsureSuccessStatusCode();
                }
                _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}, response: {1}", "url: " + url + LogUtils.GetCurrentMethod(this), ""));
            }
            
        }

        public void NetworkinterfaceDelete(string id, string macAddress)
        {
            var url = string.Format("{0}/api/virtualmachine/{1}/networkinterface?macAddress={2}", UrlApiVmm, id, macAddress);
            var putData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, putData));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.DeleteAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }

        public void NetworkinterfaceConfigure(string id, string customerCode, string macAddress, decimal bandwidth, string zabbixTemplate, string serverName, int vlanId)
        {
            var url = string.Format("{0}/api/virtualmachine/{1}/networkconfiguration?customerCode={2}&macAddress={3}&zabbixTemplate={4}&serverName={5}&bandWidth={6}&vlanid={7}", UrlApiVmm, id, customerCode, macAddress, zabbixTemplate, serverName, bandwidth, vlanId);
            
            var putData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, putData));
            using (var httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(putData);
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.PostAsync(url, content).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }
        
        public void NetworkinterfaceUpdate(string id, string customerCode, string macAddress, decimal bandwidth, string zabbixTemplate, string serverName)
        {
            var url = string.Format("{0}/api/virtualmachine/{1}/networkinterface?customerCode={2}&bandwidth={3}&zabbixTemplate={4}&serverName={5}&macAddress={6}", UrlApiVmm, id, customerCode, bandwidth, zabbixTemplate, serverName, macAddress);

            var putData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, putData));
            using (var httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(putData);
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.PutAsync(url, content).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }

        public void NetworkinterfaceRemove(string id, string macAddress)
        {
            var url = string.Format("{0}/api/virtualmachine/networkinterface/{1}?macAddress={2}", UrlApiVmm, id, macAddress);
            var putData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, putData));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.DeleteAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }

        public void DiskUpdate(string id, int size, string diskName)
        {
            var url = string.Format("{0}/api/virtualmachine/disk/{1}?size={2}&name={3}", UrlApiVmm, id, size, diskName);
            var postData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, postData));
            using (var httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(postData);
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.PutAsync(url, content).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }

        public void DiskCreate(string id, int size)
        {
            var url = string.Format("{0}/api/virtualmachine/disk/{1}?size={2}", UrlApiVmm, id, size);
            var postData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, postData));
            using (var httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(postData);
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.PostAsync(url, content).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }

        public void DiskDelete(string id, string diskName)
        {
            var url = string.Format("{0}/api/virtualmachine/disk/{1}?name={2}", UrlApiVmm, id, diskName);

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, diskName));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.DeleteAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), ""));
        }

        public VirtualmachineContract MemoryVcpuUpdate(string id, byte vcpu, int memory)
        {
            VirtualmachineContract virtualmachineContract;

            var convertMemory = memory / 1024;
            var url = string.Format("{0}/api/virtualmachine/resources/{1}?memory={2}&vcpu={3}", UrlApiVmm, id, convertMemory, vcpu);
            var postData = new List<KeyValuePair<string, string>>();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, postData));
            using (var httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(postData);
                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                HttpResponseMessage httpResponseMessage = httpClient.PostAsync(url, content).Result;
                httpResponseMessage.EnsureSuccessStatusCode();

                virtualmachineContract = JsonConvert.DeserializeObject<VirtualmachineContract>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            }
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), virtualmachineContract.ToString()));

            return virtualmachineContract;
        }
    }
}
