using System.Linq;
using Engine.Cloud.Core.Model;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Xml;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.Core.Domain.Services.AdditionalIP
{
    public class IPServerServiceXenSp : XenServiceBase, IIPServerService
    {
        public string UrlBase { get; set; }

        private readonly Server _server;
        private readonly ILogger _logger;

        public IPServerServiceXenSp(Server server)
        {
            _logger = LogFactory.GetInstance();
            _server = server;
            UrlBase = string.Format("{0}cloud/rest/", ConfigurationManager.AppSettings.Get("Cloud.XenSP1.UrlWS"));
        }

        public virtual ServiceResult CreateAdditionalIP(string networkInterfaceName)
        {
            var serviceCode = _server.ServiceCode;

            var sbBody = "";
            sbBody += "<virtualmachine>" +
                        "    <name>" + _server.Name + "</name>" +
                        "    <status>upgrade</status>" +
                        "    <networkinterfaces>" +
                        "      <networkinterface>" +
                        "        <name>" + networkInterfaceName + "</name>" +
                        "        <additionalips>" +
                        "           <additionalip inrange='true'></additionalip>" +
                        "        </additionalips>" +
                        "      </networkinterface>" +
                        "    </networkinterfaces>" +
                        "</virtualmachine>";

            UrlBase += "servicecode/" + serviceCode + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            HttpResponseMessage httpResponseMessage;
            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));

            var result = ParseXmlResult(xmlDocument);

            AppendAdditionalip(result, xmlDocument, serviceCode);

            return result;
        }

        private static void AppendAdditionalip(ServiceResult result, XmlDocument xmlDocument, Guid serviceCode)
        {
            XmlNodeList additionalips = xmlDocument.SelectNodes("/virtualmachine/networkinterfaces/networkinterface/additionalips/additionalip");

            if (additionalips == null) return;

            foreach (XmlNode node in additionalips)
            {
                var selectSingleNode = node.SelectSingleNode("memo");
                if (selectSingleNode == null) continue;

                var recentServiceCode = selectSingleNode.InnerText;
                if (recentServiceCode == serviceCode.ToString())
                {
                    result.Results.Add("additionalip", node.SelectSingleNode("ipv4").InnerText);
                    return;
                }
            }
        }

        public virtual ServiceResult DeleteAdditionalIP(string ip)
        {
            UrlBase += "servicecode/" + _server.ServiceCode + "/user/" + _server.Client.CustomerCode.Replace(".","") + "/additionalip/" + ip;
            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}", LogUtils.GetCurrentMethod(this), UrlBase));
            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(UrlBase);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.DeleteAsync(UrlBase).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><networkinterface snapshotIdentifier="39905"><identifier>5315</identifier><name>tec25057.0</name><queueitem><identifier>81952</identifier><name>DOWNGRADE</name><state>COMPLETED</state><numberOfFailures>0</numberOfFailures><serviceCode>5369e074-0e38-482c-a8a3-9dee8d1e6730</serviceCode><delegatedQueueItems /><startExecution>2014-10-21T11:11:20.419-02:00</startExecution><lastUpdated>2014-10-21T11:11:22.115-02:00</lastUpdated><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>9</expectedSteps><currentStep>8</currentStep></queueitem><additionalips><additionalip inrange="true" snapshotIdentifier="29376"><identifier>978</identifier><ipv4>177.70.97.117</ipv4></additionalip></additionalips><bandwidth>1.0</bandwidth><ipv4>177.70.97.115</ipv4><mac>00:16:3e:6b:14:7b</mac><monitor><identifier>627</identifier><user><identifier>637</identifier><name>CD119740</name><additionalips><additionalip inrange="true"><identifier>978</identifier><ipv4>177.70.97.117</ipv4></additionalip><additionalip inrange="true"><identifier>979</identifier><ipv4>177.70.98.202</ipv4></additionalip></additionalips><firewallrules /><ipranges /><vlans /></user><vlan validRange="true" firewall="true" dhcp="true"><identifier>1</identifier><name>virtbr8</name><region>TB1</region><firewallrules /><ipranges><iprange additional="false"><identifier>1</identifier><region>TB1</region><firstip>177.70.96.4</firstip><lastip>177.70.99.254</lastip></iprange><iprange additional="false"><identifier>2</identifier><firstip>192.168.168.1</firstip><lastip>192.168.168.254</lastip></iprange><iprange additional="false"><identifier>3</identifier><firstip>10.2.0.7</firstip><lastip>10.2.0.254</lastip></iprange><iprange additional="false"><identifier>4</identifier><firstip>10.1.1.7</firstip><lastip>10.1.1.254</lastip></iprange><iprange additional="false"><identifier>5</identifier><firstip>192.168.200.7</firstip><lastip>192.168.200.254</lastip></iprange><iprange additional="false"><identifier>7</identifier><firstip>192.168.41.7</firstip><lastip>192.168.41.254</lastip></iprange><iprange additional="false"><identifier>8</identifier><firstip>192.168.201.7</firstip><lastip>192.168.201.254</lastip></iprange><iprange additional="false"><identifier>9</identifier><firstip>192.168.202.7</firstip><lastip>192.168.202.254</lastip></iprange><iprange additional="false"><identifier>12</identifier><firstip>192.168.204.7</firstip><lastip>192.168.204.254</lastip></iprange><iprange additional="false"><identifier>13</identifier><firstip>192.168.205.7</firstip><lastip>192.168.205.254</lastip></iprange><iprange additional="false"><identifier>14</identifier><firstip>192.168.4.7</firstip><lastip>192.168.4.254</lastip></iprange><iprange additional="false"><identifier>15</identifier><firstip>192.168.207.7</firstip><lastip>192.168.207.254</lastip></iprange><iprange additional="false"><identifier>18</identifier><firstip>192.168.208.7</firstip><lastip>192.168.208.254</lastip></iprange><iprange additional="false"><identifier>23</identifier><firstip>192.168.56.7</firstip><lastip>192.168.56.254</lastip></iprange><iprange additional="false"><identifier>24</identifier><firstip>192.168.209.7</firstip><lastip>192.168.209.254</lastip></iprange><iprange additional="false"><identifier>26</identifier><firstip>192.168.11.7</firstip><lastip>192.168.11.254</lastip></iprange><iprange additional="false"><identifier>27</identifier><firstip>192.168.12.7</firstip><lastip>192.168.12.254</lastip></iprange><iprange additional="false"><identifier>28</identifier><firstip>192.168.211.7</firstip><lastip>192.168.211.254</lastip></iprange><iprange additional="false"><identifier>30</identifier><firstip>192.168.212.7</firstip><lastip>192.168.212.254</lastip></iprange><iprange additional="false"><identifier>31</identifier><firstip>192.168.213.7</firstip><lastip>192.168.213.254</lastip></iprange><iprange additional="false"><identifier>32</identifier><firstip>192.168.214.7</firstip><lastip>192.168.214.254</lastip></iprange><iprange additional="false"><identifier>34</identifier><firstip>192.168.14.7</firstip><lastip>192.168.14.254</lastip></iprange><iprange additional="false"><identifier>35</identifier><firstip>192.168.216.7</firstip><lastip>192.168.216.254</lastip></iprange><iprange additional="false"><identifier>36</identifier><firstip>10.0.0.7</firstip><lastip>10.0.0.254</lastip></iprange><iprange additional="false"><identifier>39</identifier><firstip>192.168.217.7</firstip><lastip>192.168.217.254</lastip></iprange><iprange additional="false"><identifier>43</identifier><firstip>192.168.221.7</firstip><lastip>192.168.221.254</lastip></iprange><iprange additional="false"><identifier>44</identifier><firstip>192.168.222.7</firstip><lastip>192.168.222.254</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>1</identifier><name>SPFW001</name><region>TB1</region><admState>regular</admState><ip>177.70.104.129</ip><externalIf>bond0.1000</externalIf><internalIf>bond0</internalIf><rate>10000Mbit</rate><secondaryVirtualIp>0.0.0.0</secondaryVirtualIp></router><vlanid>8</vlanid></vlan></monitor><vlan validRange="true" firewall="true" dhcp="true"><identifier>1</identifier><name>virtbr8</name><region>TB1</region><firewallrules /><ipranges><iprange additional="false"><identifier>1</identifier><region>TB1</region><firstip>177.70.96.4</firstip><lastip>177.70.99.254</lastip></iprange><iprange additional="false"><identifier>2</identifier><firstip>192.168.168.1</firstip><lastip>192.168.168.254</lastip></iprange><iprange additional="false"><identifier>3</identifier><firstip>10.2.0.7</firstip><lastip>10.2.0.254</lastip></iprange><iprange additional="false"><identifier>4</identifier><firstip>10.1.1.7</firstip><lastip>10.1.1.254</lastip></iprange><iprange additional="false"><identifier>5</identifier><firstip>192.168.200.7</firstip><lastip>192.168.200.254</lastip></iprange><iprange additional="false"><identifier>7</identifier><firstip>192.168.41.7</firstip><lastip>192.168.41.254</lastip></iprange><iprange additional="false"><identifier>8</identifier><firstip>192.168.201.7</firstip><lastip>192.168.201.254</lastip></iprange><iprange additional="false"><identifier>9</identifier><firstip>192.168.202.7</firstip><lastip>192.168.202.254</lastip></iprange><iprange additional="false"><identifier>12</identifier><firstip>192.168.204.7</firstip><lastip>192.168.204.254</lastip></iprange><iprange additional="false"><identifier>13</identifier><firstip>192.168.205.7</firstip><lastip>192.168.205.254</lastip></iprange><iprange additional="false"><identifier>14</identifier><firstip>192.168.4.7</firstip><lastip>192.168.4.254</lastip></iprange><iprange additional="false"><identifier>15</identifier><firstip>192.168.207.7</firstip><lastip>192.168.207.254</lastip></iprange><iprange additional="false"><identifier>18</identifier><firstip>192.168.208.7</firstip><lastip>192.168.208.254</lastip></iprange><iprange additional="false"><identifier>23</identifier><firstip>192.168.56.7</firstip><lastip>192.168.56.254</lastip></iprange><iprange additional="false"><identifier>24</identifier><firstip>192.168.209.7</firstip><lastip>192.168.209.254</lastip></iprange><iprange additional="false"><identifier>26</identifier><firstip>192.168.11.7</firstip><lastip>192.168.11.254</lastip></iprange><iprange additional="false"><identifier>27</identifier><firstip>192.168.12.7</firstip><lastip>192.168.12.254</lastip></iprange><iprange additional="false"><identifier>28</identifier><firstip>192.168.211.7</firstip><lastip>192.168.211.254</lastip></iprange><iprange additional="false"><identifier>30</identifier><firstip>192.168.212.7</firstip><lastip>192.168.212.254</lastip></iprange><iprange additional="false"><identifier>31</identifier><firstip>192.168.213.7</firstip><lastip>192.168.213.254</lastip></iprange><iprange additional="false"><identifier>32</identifier><firstip>192.168.214.7</firstip><lastip>192.168.214.254</lastip></iprange><iprange additional="false"><identifier>34</identifier><firstip>192.168.14.7</firstip><lastip>192.168.14.254</lastip></iprange><iprange additional="false"><identifier>35</identifier><firstip>192.168.216.7</firstip><lastip>192.168.216.254</lastip></iprange><iprange additional="false"><identifier>36</identifier><firstip>10.0.0.7</firstip><lastip>10.0.0.254</lastip></iprange><iprange additional="false"><identifier>39</identifier><firstip>192.168.217.7</firstip><lastip>192.168.217.254</lastip></iprange><iprange additional="false"><identifier>43</identifier><firstip>192.168.221.7</firstip><lastip>192.168.221.254</lastip></iprange><iprange additional="false"><identifier>44</identifier><firstip>192.168.222.7</firstip><lastip>192.168.222.254</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>1</identifier><name>SPFW001</name><region>TB1</region><admState>regular</admState><ip>177.70.104.129</ip><externalIf>bond0.1000</externalIf><internalIf>bond0</internalIf><rate>10000Mbit</rate><secondaryVirtualIp>0.0.0.0</secondaryVirtualIp></router><vlanid>8</vlanid></vlan></networkinterface>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><queueitem><identifier>0</identifier><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>5369e074-0e38-482c-a8a3-9dee8d1e6730</serviceCode><delegatedQueueItems /><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: No additional IP 177.70.98.202 found to delete</message></queueitem></virtualmachine>
            #endregion
        }

        public virtual ServiceResult CreateVip(Server serverTarget, string newIp, string networkInterfaceName)
        {
            //UrlBase += "servicecode/" + serverTarget.ServiceCode + "/virtualmachine/" + serverTarget.Name + "/Additionalip/" + newIp;
            UrlBase += "servicecode/" + serverTarget.ServiceCode + "/networkinterface/" + networkInterfaceName + "/Additionalip/" + newIp;
            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}", LogUtils.GetCurrentMethod(this), UrlBase));
            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent("application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));

            return ParseXmlResult(xmlDocument);
        }

        public ServiceResult CreateNetworkinterface(decimal bandwidth, int vlanId, string zabbixTemplate)
        {
            var sbBody = "";
            
            if (vlanId > 0)
            {
                sbBody += "<virtualmachine>";
                sbBody += "     <name>" + _server.Name + "</name>";
                sbBody += "     <status>upgrade</status>";
                sbBody += "     <networkinterfaces>";
                sbBody += "         <networkinterface>";
                sbBody += "             <vlan>";
                sbBody += "                 <vlanid>" + vlanId + "</vlanid>";
                sbBody += "             </vlan>";
                sbBody += "         </networkinterface>";
                sbBody += "     </networkinterfaces>";
                sbBody += "</virtualmachine>";
            }
            else
            {
                sbBody += "<virtualmachine>";
                sbBody += "     <name>" + _server.Name + "</name>";
                sbBody += "     <status>upgrade</status>";
                sbBody += "     <networkinterfaces>";
                sbBody += "         <networkinterface>";
                sbBody += "             <bandwidth>" + bandwidth + "</bandwidth>";
                sbBody += "         </networkinterface>";
                sbBody += "     </networkinterfaces>";
                sbBody += "</virtualmachine>";
            }

            UrlBase += "servicecode/" + Guid.NewGuid() + "/virtualmachine";

            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));
            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);

            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseQueueitemVirtualMachineState(xmlDocument);
        }

        public ServiceResult DeleteNetworkinterface(string macAddress)
        {
            macAddress = string.IsNullOrEmpty(macAddress) ? _server.Resources.NetworkInterfaces[0].Mac : macAddress;

            if (!_server.Resources.NetworkInterfaces.Any())
                return new ServiceResult()
                {
                    Status = StatusService.SUCCESS,
                    Result = "no_network_interface"
                };

            var url = UrlBase + "servicecode/" + _server.ServiceCode + "/networkinterface/" + macAddress;

            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, string.Empty));
            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {

                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.DeleteAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);

            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseQueueitemNetworkInterfaceState(xmlDocument);
        }

        public ServiceResult ConfigureNetworkinterface(string macAddress, decimal bandwidth, string zabbixTemplate, int vlanId)
        {
            macAddress = string.IsNullOrEmpty(macAddress) ? _server.Resources.NetworkInterfaces[0].Mac : macAddress;

            var sbBody = "";

            if (vlanId > 0)
            {
                sbBody += "<virtualmachine>";
                sbBody += "<identifier>" + _server.HypervisorIdentifier + "</identifier>";
                sbBody += "<queueitem mode=\"asynchronous\"><name>UPGRADE</name></queueitem>";
                sbBody += "	<networkinterfaces>";
                sbBody += "		<networkinterface>";
                sbBody += "			<vlan>";
                sbBody += "				<vlanid>" + vlanId + "</vlanid>";
                sbBody += "			</vlan>";
                sbBody += "		</networkinterface>";
                sbBody += "	</networkinterfaces>";
                sbBody += "</virtualmachine>";
            }
            else
            {
                sbBody += "<networkinterface>";
                sbBody += "     <bandwidth>" + bandwidth + "</bandwidth>";
                sbBody += "     <monitor><user><name>" + _server.Client.CustomerCode.ToUpper().Replace(".", "") + "</name></user></monitor>";
                sbBody += "     <mac>" + macAddress + "</mac>";
                sbBody += "</networkinterface>";
            }
            
            UrlBase += "servicecode/" + _server.ServiceCode + "/networkinterface";

            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));
            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.PostAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);

            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseQueueitemNetworkInterfaceState(xmlDocument);
        }

        public ServiceResult UnRegisterNetworkInterface(string macAddress)
        {
            macAddress = string.IsNullOrEmpty(macAddress) ? _server.Resources.NetworkInterfaces[0].Mac : macAddress;

            if (!_server.Resources.NetworkInterfaces.Any())
                return new ServiceResult()
                {
                    Status = StatusService.SUCCESS,
                    Result = "no_network_interface"
                };


            if (this.GetNetworkDetailsByMac(macAddress) == null)
            {
                return new ServiceResult()
                {
                    Status = StatusService.SUCCESS,
                    Result = "mac_ip_dont_exists"
                };
            }

            var url = UrlBase + "servicecode/" + _server.ServiceCode + "/networkinterface/" + macAddress;

            var xmlDocument = new XmlDocument();

            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, string.Empty));
            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {

                httpClient.BaseAddress = new Uri(url);
                httpClient.Timeout = TimeSpan.FromMinutes(HTTP_TIMEOUT);
                httpResponseMessage = httpClient.DeleteAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);

            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseQueueitemNetworkInterfaceState(xmlDocument);
        }

        public ServiceResult UpdateNetworkInterface(string macAddress, decimal bandwidth, string zabbixTemplate, string serverName)
        {
            throw new NotImplementedException();
        }

        public ServiceResult GetNetworkDetailsByMac(string mac)
        {
            if (mac == null) return null;

            var url = UrlBase + "networkinterface/" + mac;

            var result = new ServiceResult();
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;

            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(url);
                httpResponseMessage = httpClient.GetAsync(url).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);

            if (xmlDocument.SelectSingleNode("networkinterface/ipv4") != null)
            {
                result.Results.Add("ipv4", xmlDocument.SelectSingleNode("networkinterface/ipv4").InnerText);
                result.Results.Add("name", xmlDocument.SelectSingleNode("networkinterface/name").InnerText);
                result.Results.Add("bandwidth", xmlDocument.SelectSingleNode("networkinterface/bandwidth").InnerText);
            }
            else
                return null;

            return result;
        }

        public ServiceResult LoadNetworkInterface(string macAddress)
        {
            throw new NotImplementedException();
        }
    }
}