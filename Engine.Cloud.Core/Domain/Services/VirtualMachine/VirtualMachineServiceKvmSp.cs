using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml;
using Engine.Cloud.Core.Domain.Services.VirtualMachine.Mappers;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Model.ServerHosts;
using Engine.Cloud.Core.Utils.Logging;
using Utils;
using Newtonsoft.Json;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine
{
    public class VirtualMachineServiceKvmSp : XenServiceBase, IVirtualMachineService
    {
        public virtual string UrlBase { get; set; }
        private readonly ILogger _logger;

        public VirtualMachineServiceKvmSp()
        {
            UrlBase = ConfigurationManager.AppSettings.Get("Cloud.KvmSP1.UrlWS");
            _logger = new Log4NetAdapter();
        }

        public void Load(Server server)
        {
            UrlBase += "kratos/rest/virtualmachine/" + server.HypervisorIdentifier;

            var xmlDocument = new XmlDocument();

            try
            {
                HttpResponseMessage httpResponseMessage;

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(UrlBase);
                    httpClient.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT);
                    httpResponseMessage = httpClient.GetAsync(UrlBase).Result;
                    if ((httpResponseMessage.StatusCode == HttpStatusCode.NotFound) || (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError))
                        server.RemoteStatus = RemoteStatus.DontExist;

                    httpResponseMessage.EnsureSuccessStatusCode();
                }

                xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
                new MapperVirtualMachineServiceXenSp(xmlDocument).MapperToServer(server);
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}, request: {1}", LogUtils.GetCurrentMethod(this), UrlBase), ex);
                throw;
            }
        }

        public Server Load(string hypervisorIdentifier) // todo: faz a mesma coisa do de cima
        {
            UrlBase += "kratos/rest/virtualmachine/" + hypervisorIdentifier;

            var xmlDocument = new XmlDocument();

            try
            {
                HttpResponseMessage httpResponseMessage;

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(UrlBase);
                    httpClient.Timeout = TimeSpan.FromSeconds(HTTP_TIMEOUT * 10);
                    httpResponseMessage = httpClient.GetAsync(UrlBase).Result;

                    httpResponseMessage.EnsureSuccessStatusCode();
                }

                var server = new Server();
                xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
                new MapperVirtualMachineServiceXenSp(xmlDocument).MapperToServer(server);
                return server;
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}, request: {1}", LogUtils.GetCurrentMethod(this), UrlBase), ex);
                throw;
            }
        }

        public void LoadAll(List<Server> servers)
        {
            UrlBase += "kratos/rest/maintenance/virtualmachine/all";

            var xmlDocument = new XmlDocument();

            try
            {
                HttpResponseMessage httpResponseMessage;

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(UrlBase);
                    httpClient.Timeout = TimeSpan.FromSeconds(999999);
                    httpResponseMessage = httpClient.GetAsync(UrlBase).Result;
                    httpResponseMessage.EnsureSuccessStatusCode();
                }

                xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
                new MapperVirtualMachineServiceXenSp(xmlDocument).MapperToServers(servers);
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}, request: {1}", LogUtils.GetCurrentMethod(this), UrlBase), ex);
                throw;
            }
        }

        public ServiceResult Install(Client client, byte vcpu, int frequency, int memory, string image, int diskSize, decimal[] bandwidths, int partitions, string formatDisk, long serverId)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<vcpu>" + vcpu + "</vcpu>";
            sbBody += "<memory>" + (memory * 1024 * 1024) + "</memory>";
            sbBody += "<frequency>" + frequency + "</frequency>";
            sbBody += "<software>";
            sbBody += "<name>" + image + "</name>";

            if (partitions == 1)
                sbBody += "<size>" + diskSize + "</size>";
            else
                sbBody += "<size>" + (image.Contains("windows") == true ? 32 : 10) + "</size>";

            sbBody += "</software>";
            sbBody += "<queueitem>";
            sbBody += "<name>INSTALL</name>";
            sbBody += "<serviceCode>" + Guid.NewGuid() + "</serviceCode>";
            sbBody += "<returnUrl></returnUrl>";
            sbBody += "</queueitem>";
            sbBody += "<user>";
            sbBody += "<name>" + client.CustomerCode.Replace(".", "") + "</name>";
            sbBody += "</user>";
            sbBody += "<networkinterfaces>";

            foreach (var bandwidth in bandwidths)
            {
                sbBody += "<networkinterface>";
                if (bandwidth > 0)
                    sbBody += "<bandwidth>" + bandwidth.ToString(CultureInfo.InvariantCulture).Replace(",", ".") + "</bandwidth>";
                else
                    sbBody += "<bandwidth></bandwidth>";
                sbBody += "</networkinterface>";
            }

            sbBody += "</networkinterfaces>";

            if (partitions != 1)
            {
                sbBody += "<disks>";
                sbBody += "<disk>";
                sbBody += "<size>" + (diskSize - (image.Contains("windows") == true ? 32 : 10)) + "</size>";
                if (formatDisk != "")
                    sbBody += "<fileSystem>" + formatDisk + "</fileSystem>";
                sbBody += "</disk>";
                sbBody += "</disks>";
            }
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/VirtualMachineInstall";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

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
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="249"><identifier>87</identifier><name>CD119740-linux-centos-6-64b-base-87</name><queueitem><identifier>719</identifier><name>INSTALL</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>414397a1-2afb-4fd0-90e0-ceee1f8b8cc6</serviceCode><returnUrl>http://127.0.0.1:8080/kratos/rest/PortalDelivery</returnUrl><scope>VirtualMachine</scope><delegatedQueueItems><queueitem><identifier>720</identifier><name>CONFIGURE</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><scope>NetworkInterface</scope><delegatedQueueItems /><startExecution>2014-10-23T10:43:05.472-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem></delegatedQueueItems><startExecution>2014-10-23T10:43:05.363-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>true</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><disks><disk snapshotIdentifier="474"><identifier>122</identifier><name>d122</name><device>vda</device><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="255"><identifier>88</identifier><name>tec87.0</name><queueitem><identifier>720</identifier><name>CONFIGURE</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><scope>NetworkInterface</scope><delegatedQueueItems /><startExecution>2014-10-23T10:43:05.472-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><bandwidth>1.0</bandwidth><ipv4>177.70.105.32</ipv4><mac>00:16:3e:ec:65:30</mac><monitor><identifier>2</identifier><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user><vlan dhcp="true" firewall="true" validRange="true"><identifier>1</identifier><name>virtbr12</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>1</identifier><region>TB2</region><firstip>177.70.105.10</firstip><lastip>177.70.105.74</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>1</identifier><name>spc1hfw1</name><region>TB2</region><ip>177.70.105.1</ip><externalIf>eth0</externalIf><internalIf>eth0</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.105.2</secondaryVirtualIp></router><vlanid>12</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="true"><identifier>1</identifier><name>virtbr12</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>1</identifier><region>TB2</region><firstip>177.70.105.10</firstip><lastip>177.70.105.74</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>1</identifier><name>spc1hfw1</name><region>TB2</region><ip>177.70.105.1</ip><externalIf>eth0</externalIf><internalIf>eth0</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.105.2</secondaryVirtualIp></router><vlanid>12</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user><vcpu>2</vcpu></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><queueitem><identifier>0</identifier><name>INSTALL</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>414397a1-2afb-4fd0-90e0-ceee1f8b8cc6</serviceCode><returnUrl>http://127.0.0.1:8080/kratos/rest/PortalDelivery</returnUrl><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T10:43:31.441-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem><frequency>900</frequency><memory>512000</memory><networkinterfaces><networkinterface><bandwidth>1.0</bandwidth></networkinterface></networkinterfaces><software><name>linux-centos-6-64b-base</name></software><user><name>CD119740</name></user><vcpu>2</vcpu></virtualmachine>
            #endregion
        }

        public ServiceResult Uninstall(Server server)
        {
            UrlBase += "kratos/rest/servicecode/" + Guid.NewGuid() + "/virtualmachine/" + server.Name;
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}", LogUtils.GetCurrentMethod(this), UrlBase));

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
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="254"><identifier>87</identifier><queueitem><identifier>739</identifier><name>UNINSTALL</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>477aec2c-bc78-4161-8dbc-3e65a4219f75</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems><queueitem><identifier>740</identifier><name>DOWNGRADE</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><scope>NetworkInterface</scope><delegatedQueueItems /><startExecution>2014-10-23T11:13:42.845-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><queueitem><identifier>741</identifier><name>REMOVE</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><scope>Disk</scope><delegatedQueueItems /><startExecution>2014-10-23T11:13:42.854-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem></delegatedQueueItems><startExecution>2014-10-23T11:13:42.739-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>87</identifier><queueitem><identifier>0</identifier><name>UNINSTALL</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>477aec2c-bc78-4161-8dbc-3e65a4219f75</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T11:14:27.005-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult Reinstall(Server server, Image image)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>reinstall</status>";
            sbBody += "<user><name>" + server.Client.CustomerCode.Replace(".", "") + "</name></user>";
            sbBody += "<software>";
            sbBody += "<name>" + image.Name + "</name>";
            sbBody += "</software>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="290"><identifier>89</identifier><name>CD119740-linux-centos-6-64b-base-89</name><queueitem><identifier>837</identifier><name>REINSTALL</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>ea974950-11a1-476b-a001-ef5b0f5cd32f</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems><queueitem><identifier>838</identifier><name>REMOVE</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><scope>Disk</scope><delegatedQueueItems /><startExecution>2014-10-23T16:10:25.609-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem></delegatedQueueItems><startExecution>2014-10-23T16:10:25.318-02:00</startExecution><doPoweroff>true</doPoweroff><doPoweron>true</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_ON</state><disks><disk snapshotIdentifier="593"><identifier>134</identifier><name>d134</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12610730721280</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>running</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1645</frequency><loadAverage>2.39</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12610730721280</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4474885111808</freeSize><internalIp>10.15.0.2</internalIp><size>4492</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="300"><identifier>90</identifier><name>tec89.0</name><bandwidth>1.0</bandwidth><ipv4>177.70.105.34</ipv4><mac>00:16:3e:8a:0a:08</mac><monitor><identifier>2</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="true"><identifier>1</identifier><name>virtbr12</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>1</identifier><region>TB2</region><firstip>177.70.105.10</firstip><lastip>177.70.105.74</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>1</identifier><name>spc1hfw1</name><region>TB2</region><ip>177.70.105.1</ip><externalIf>eth0</externalIf><internalIf>eth0</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.105.2</secondaryVirtualIp></router><vlanid>12</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="true"><identifier>1</identifier><name>virtbr12</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>1</identifier><region>TB2</region><firstip>177.70.105.10</firstip><lastip>177.70.105.74</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>1</identifier><name>spc1hfw1</name><region>TB2</region><ip>177.70.105.1</ip><externalIf>eth0</externalIf><internalIf>eth0</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.105.2</secondaryVirtualIp></router><vlanid>12</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12610730721280</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>Fyp5Wkevo3</vncPasswd><vncPort>5900</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>89</identifier><queueitem><identifier>0</identifier><name>REINSTALL</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>ea974950-11a1-476b-a001-ef5b0f5cd32f</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T16:10:47.405-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem><software><name>linux-centos-6-64b-base</name></software></virtualmachine>
            #endregion
        }

        public ServiceResult PowerOn(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>poweron</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="175"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>456</identifier><name>POWERON</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>3e89ece9-a1a7-4f9d-a43a-344a5fd1f21f</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T14:51:45.875-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_ON</state><disks><disk snapshotIdentifier="327"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12653969207296</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>running</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.02</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12653969207296</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4626506055680</freeSize><internalIp>10.15.0.2</internalIp><size>4628</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="176"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12653969207296</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>POWERON</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>3e89ece9-a1a7-4f9d-a43a-344a5fd1f21f</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T14:52:08.547-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult PowerOff(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>poweroff</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="238"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>697</identifier><name>SHUTDOWN</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>76a2eb71-9e53-458e-8492-ea3c3fc904b4</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:56:25.451-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_ON</state><disks><disk snapshotIdentifier="451"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506695168</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>failure</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.03</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506695168</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4614331039744</freeSize><internalIp>10.15.0.2</internalIp><size>4619</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="244"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506695168</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>SHUTDOWN</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>76a2eb71-9e53-458e-8492-ea3c3fc904b4</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:57:03.031-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult ShutDown(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>shutdown</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="172"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>453</identifier><name>POWEROFF</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>36b799cd-353d-41c1-89bc-b02e92b11787</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T14:43:44.709-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_OFF</state><disks><disk snapshotIdentifier="324"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12653969207296</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>running</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.03</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12653969207296</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4626512347136</freeSize><internalIp>10.15.0.2</internalIp><size>4628</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="173"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12653969207296</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>POWEROFF</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>36b799cd-353d-41c1-89bc-b02e92b11787</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T14:44:14.691-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult Reboot(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>reboot</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));


            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="237"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>696</identifier><name>REBOOT</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>43e11776-d9cb-4e3a-8746-c08f0e032651</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:26:56.521-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_ON</state><disks><disk snapshotIdentifier="450"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>failure</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.02</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4615573602304</freeSize><internalIp>10.15.0.2</internalIp><size>4621</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="243"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>REBOOT</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>43e11776-d9cb-4e3a-8746-c08f0e032651</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:27:28.132-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult Suspend(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<memo>Suspensão realizada pelo painel cloud</memo>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/action/block/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="276" lock="SUSPENDED"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>784</identifier><name>BLOCK</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>3766dacf-2495-4165-95fb-2dc3eda99894</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T14:42:03.214-02:00</startExecution><doPoweroff>true</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_OFF</state><disks><disk snapshotIdentifier="556"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626291658752</freeSize><size>13402</size></storage></disk><disk snapshotIdentifier="557"><identifier>119</identifier><name>d119</name><state>CREATED</state><device>vdb</device><diskType>DATA</diskType><format>qcow2</format><size>50</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626291658752</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user></disk><disk snapshotIdentifier="558"><identifier>120</identifier><name>d120</name><state>CREATED</state><device>vdc</device><diskType>DATA</diskType><format>qcow2</format><size>60</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626291658752</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user></disk><disk snapshotIdentifier="559"><identifier>126</identifier><name>d126</name><device>vdd</device><diskType>DATA</diskType><format>qcow2</format><size>50</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626291658752</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>running</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1645</frequency><loadAverage>2.03</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626291658752</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4551104004096</freeSize><internalIp>10.15.0.2</internalIp><size>4563</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memo>Suspensão realizada pelo painel cloud</memo><memory>1048576</memory><networkinterfaces><networkinterface snapshotIdentifier="282"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626291658752</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5901</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>BLOCK</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>3766dacf-2495-4165-95fb-2dc3eda99894</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T14:43:10.301-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem><memo>Suspensão realizada pelo painel cloud</memo></virtualmachine>
            #endregion
        }

        public ServiceResult Unlock(Server server)
        {
            var sbBody = "";
            sbBody += "<collection>";
            sbBody += "<virtualmachine>";
            sbBody += "<name>" + server.Name + "</name>";
            sbBody += "</virtualmachine>";
            sbBody += "</collection>";

            UrlBase += "kratos/rest/virtualmachine/setobjectlock/ok";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));


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

            if (xmlDocument.InnerXml.Contains("virtualmachine lock=\"SUSPENDED\"") == false)
                return new ServiceResult() { Status = StatusService.SUCCESS };

            return new ServiceResult() { Status = StatusService.FAILED };

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><collection><VirtualMachine><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><state>POWERED_OFF</state><disks><disk><identifier>100</identifier><name>d100</name><device>vda</device><format>qcow2</format><restorepoints /><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626795110400</freeSize><size>13402</size></storage></disk><disk><identifier>119</identifier><name>d119</name><state>CREATED</state><device>vdb</device><format>qcow2</format><restorepoints /><size>50</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626795110400</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user></disk><disk><identifier>120</identifier><name>d120</name><state>CREATED</state><device>vdc</device><format>qcow2</format><restorepoints /><size>60</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626795110400</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user></disk><disk><identifier>126</identifier><name>d126</name><device>vdd</device><format>qcow2</format><restorepoints /><size>50</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626795110400</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>running</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1645</frequency><loadAverage>2.1</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12626795110400</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4547503194112</freeSize><internalIp>10.15.0.2</internalIp><size>4560</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>1048576</memory><networkinterfaces><networkinterface><identifier>74</identifier><name>tec74.0</name><additionalips /><bandwidth>1.0</bandwidth><firewallrules /><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><user><identifier>3</identifier><name>CD119740</name><additionalips /><firewallrules /><ipranges /><vlans /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5901</vncPort></VirtualMachine></collection>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>UNBLOCK</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>3766dacf-2495-4165-95fb-2dc3eda99894</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T14:43:10.301-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem><memo>Suspensão realizada pelo painel cloud</memo></virtualmachine>
            #endregion
        }

        public ServiceResult Resume(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>resume</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="235"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>694</identifier><name>RESUME</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>37f31f90-675e-40bf-8f17-6d55d41841b2</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:18:44.416-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_ON</state><disks><disk snapshotIdentifier="448"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>failure</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.01</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4615894466560</freeSize><internalIp>10.15.0.2</internalIp><size>4621</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="241"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>RESUME</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>37f31f90-675e-40bf-8f17-6d55d41841b2</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:19:13.932-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult Reset(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>reset</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));


            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="236"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>695</identifier><name>RESET</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>0a3fbfba-5f6f-4217-a41c-0d74c1bbb5b3</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:22:04.035-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_ON</state><disks><disk snapshotIdentifier="449"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>failure</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.02</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4615730888704</freeSize><internalIp>10.15.0.2</internalIp><size>4621</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="242"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>RESET</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>0a3fbfba-5f6f-4217-a41c-0d74c1bbb5b3</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:22:40.889-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult Pause(Server server)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>suspend</status>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), UrlBase, sbBody));

            using (var httpClient = new HttpClient())
            {
                HttpContent myContent = new StringContent(sbBody, Encoding.UTF8, "application/xml");
                httpClient.BaseAddress = new Uri(UrlBase);
                httpResponseMessage = httpClient.PutAsync(UrlBase, myContent).Result;
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            xmlDocument.LoadXml(httpResponseMessage.Content.ReadAsStringAsync().Result);
            _logger.Log(string.Format("{0}, response: {1}", LogUtils.GetCurrentMethod(this), xmlDocument.OuterXml));
            return ParseXmlResult(xmlDocument);

            #region SUCESS RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="234"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>693</identifier><name>SUSPEND</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>1cd8791e-311f-422e-8016-96c2e8e14058</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:15:39.868-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>SUSPENDED</state><disks><disk snapshotIdentifier="447"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>failure</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>1207</frequency><loadAverage>2.05</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4616020295680</freeSize><internalIp>10.15.0.2</internalIp><size>4621</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>512000</memory><networkinterfaces><networkinterface snapshotIdentifier="240"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637505712128</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5911</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>SUSPEND</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>1cd8791e-311f-422e-8016-96c2e8e14058</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-22T18:15:58.691-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem></virtualmachine>
            #endregion
        }

        public ServiceResult Refresh(Server server)
        {
            throw new NotImplementedException();
        }
        
    }
}