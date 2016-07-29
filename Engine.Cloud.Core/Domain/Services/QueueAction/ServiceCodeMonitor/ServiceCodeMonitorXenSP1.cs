using System;
using System.Collections.Generic;
using System.Xml;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using System.Configuration;
using System.Net.Http;
using Utils;

namespace Engine.Cloud.Core.Domain.Services.QueueAction.ServiceCodeMonitor
{
    public class ServiceCodeMonitorXenSP1 : XenServiceBase, IServiceCodeMonitor
    {
        private readonly ILogger _logger;
        public string UrlBase { get; set; }

        public ServiceCodeMonitorXenSP1()
        {
            UrlBase = string.Format("{0}cloud/rest/", ConfigurationManager.AppSettings.Get("Cloud.XenSP1.UrlWS"));
            _logger = LogFactory.GetInstance();
        }


        public ServiceResult Get(string serviceCode)
        {
            var url = UrlBase + "queueitem/servicecode/" + serviceCode;

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

                var content = httpResponseMessage.Content.ReadAsStringAsync().Result;

                if (!content.IsValidXml())
                {// por algum motivo, api retorna vazio. não vou entrar no merito.
                    _logger.Log(string.Format("retorno de servicecode inválido : {0}.",content));
                    return new ServiceResult() {Result = "ON_QUEUE", Status = StatusService.SUCCESS};
                }
                
                if (content.Contains("NOT_FOUND"))
                    throw new Exception("SC_NOT_FOUND");

                xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
                var result = GetQueueItem_State(xmlDocument);
                var results = new Dictionary<string, string>
                {
                    {"serverName", GetQueueItemServerName(xmlDocument)},
                    {"hypervisorIdentifier", GetQueueItemHypervisorIdentifier(xmlDocument)},
                    {"ip", GetQueueItemIp(xmlDocument)}
                };

                if (result.Contains("FAILED"))
                {
                    return new ServiceResult {Result = GetQueueItem_State(xmlDocument), Status = StatusService.SUCCESS};
                }
                if (result == "ROLLED_BACK")
                {
                    return new ServiceResult() {Result = "ROLLED_BACK", Status = StatusService.SUCCESS};
                }
                else if (result == "COMPLETED")
                {
                    return new ServiceResult() {Result = result, Status = StatusService.SUCCESS, Results = results};
                }

                return new ServiceResult() {Result = "ON_QUEUE", Status = StatusService.SUCCESS};

            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("Erro ao buscar servicecode {0} - {1}", url, ex));
                return new ServiceResult() {Result = ex.Message, Status = StatusService.FAILED};
            }
        }
    }
}
