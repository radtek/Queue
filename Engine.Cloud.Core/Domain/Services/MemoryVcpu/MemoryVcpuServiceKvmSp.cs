using System;
using Engine.Cloud.Core.Model;
using System.Net.Http;
using System.Configuration;
using System.Text;
using System.Xml;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils.Logging;
using Utils;

namespace Engine.Cloud.Core.Domain.Services.MemoryVcpu
{
    public class MemoryVcpuServiceKvmSp : XenServiceBase, IMemoryVcpuService
    {
        private readonly Server _server;
        private readonly ILogger _logger;
        public virtual string UrlBase { get; set; }

        public MemoryVcpuServiceKvmSp(Server server)
        {
            _logger = LogFactory.GetInstance();
            UrlBase = ConfigurationManager.AppSettings.Get("Cloud.KvmSP1.UrlWS");
            _server = server;
        }

        public ServiceResult Change(byte vcpu, int frequency, int memory)
        {
            var sbBody = "";
            sbBody += "<virtualmachine>";
            sbBody += "<identifier>" + _server.HypervisorIdentifier + "</identifier>";
            sbBody += "<status>upgrade</status>";
            sbBody += "<vcpu>" + vcpu + "</vcpu>";
            sbBody += "<frequency>" + frequency + "</frequency>";
            sbBody += "<memory>" + memory + "</memory>";
            sbBody += "</virtualmachine>";

            UrlBase += "kratos/rest/async/servicecode/" + Guid.NewGuid() + "/virtualmachine";
            var xmlDocument = new XmlDocument();

            HttpResponseMessage httpResponseMessage;

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
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine snapshotIdentifier="247"><identifier>74</identifier><name>CD119740-linux-centos-6-64b-base-74</name><queueitem><identifier>717</identifier><name>UPGRADE</name><state>ON_QUEUE</state><numberOfFailures>0</numberOfFailures><serviceCode>2bc15b02-e78b-48c0-aa1d-b3c22ece4abe</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T09:41:31.842-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep></queueitem><state>POWERED_OFF</state><disks><disk snapshotIdentifier="468"><identifier>100</identifier><name>d100</name><device>vda</device><diskType>SO</diskType><format>qcow2</format><size>20</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>failure</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage></disk><disk snapshotIdentifier="469"><identifier>119</identifier><name>d119</name><device>vdb</device><diskType>DATA</diskType><format>qcow2</format><size>50</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>failure</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user></disk><disk snapshotIdentifier="470" lock="EXECUTION"><identifier>120</identifier><name>d120</name><state>CREATED</state><device>vdc</device><diskType>DATA</diskType><format>qcow2</format><size>50</size><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>failure</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user></disk></disks><frequency>900</frequency><host><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>3</keepalive><opState>running</opState><port>5200</port><technology>qemu</technology><version>17</version><frequency>2051</frequency><loadAverage>2.0</loadAverage><memory>387773</memory><sshKey>ssh-dss AAAAB3NzaC1kc3MAAACBAOKxEMml/csM8wpYn9XNQo+kKVT1oj9yyOu5/MGCPKgkwlolmuVOnKVaOcMP0NWTf9bym26CjlNDaEygNZ1xmw+COl319vyVIYenGpaXTprok/Yk0DYIZNP2dEWCYL30GEaCYzhpcigC+DbziVNU2R7QceAL7dTaTqgfYiec0qZpAAAAFQD5BmZdv1uFYFGX1/UMsPZCcTJiNQAAAH8fA1PMQlNsUJOXb02K6A6d2WXQ2ftuGjX47ThxwQnWT+wQwRc7N6xtYran4vRxIqxLtsDQJqrWaYBsQ6Qalo5igzh07Q++yS93adOwvkXx7Rr6c+s7kfOCbNVt5gucOPw2W8Yk+TU2IN/+2N0IzsbuOOVwky8D64+j3yykGL6rAAAAgQDcaZ3SEExcBmj7/cjJPxZkZ4TcOfggkyCWDmZRbo3JMk0UK1eYnK3RnctYz4zrk9Gmy7YjP23zm4PKHtc/rWFAmj0eO9dM9fRshgWtz8hD6HMgQqfGhiV8NX+5eCRhXJTTC25GGyi9qutowD7al7dyTSVtgvTKczpauVupjUUSxg== mamorim@frajjolaa</sshKey><storages><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>failure</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage><storage><flags>SOFTWARE_REPOSITORY</flags><identifier>2</identifier><name>templates</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>running</opState><port>5200</port><technology>nfs</technology><version></version><freeSize>4576227885056</freeSize><internalIp>10.15.0.2</internalIp><size>4587</size><sourcePath>/export/tplkratos</sourcePath></storage></storages><vcpu>16</vcpu></host><memory>2097152</memory><networkinterfaces><networkinterface snapshotIdentifier="253"><identifier>74</identifier><name>tec74.0</name><bandwidth>1.0</bandwidth><ipv4>10.17.0.26</ipv4><mac>00:16:3e:53:cb:80</mac><monitor><identifier>4</identifier><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></monitor><vlan dhcp="true" firewall="true" validRange="false"><flags>BACKEND</flags><identifier>3</identifier><name>vlan4</name><region>TB2</region><firewallrules /><ipranges><iprange additional="false"><identifier>2</identifier><region>TB2</region><firstip>10.17.0.20</firstip><lastip>10.17.0.30</lastip></iprange></ipranges><networkPrefixSize>0</networkPrefixSize><router><identifier>2</identifier><name>ceph_mon</name><region>TB2</region><ip>177.70.104.47</ip><externalIf>eth0</externalIf><internalIf>eth1</internalIf><rate>1000Mbit</rate><secondaryVirtualIp>177.70.104.47</secondaryVirtualIp></router><vlanid>4</vlanid></vlan></networkinterface></networkinterfaces><software><identifier>2</identifier><name>linux-centos-6-64b-base</name><fileSystem>ext3</fileSystem><initialDisks><initialDisk><identifier>3</identifier><diskName>vda</diskName><diskType>SO</diskType><format>qcow2</format></initialDisk></initialDisks><size>20</size><user><flags>SOFTWARE_PUBLISHER</flags><identifier>1</identifier><name>CD117497</name><additionalips /><firewallrules /><ipranges /><vlans /></user><version>1</version></software><softwareVersion>1</softwareVersion><storage><identifier>1</identifier><name>spk1h001</name><region>TB2</region><admState>regular</admState><ip>177.70.105.132</ip><keepalive>5</keepalive><opState>failure</opState><port>5200</port><technology>local</technology><version></version><freeSize>12637506412544</freeSize><size>13402</size></storage><user><identifier>3</identifier><name>CD119740</name><ipranges /></user><vcpu>2</vcpu><vncPasswd>sZa881bEtg</vncPasswd><vncPort>5900</vncPort></virtualmachine>
            #endregion

            #region FAIL RETURN
            //<?xml version="1.0" encoding="UTF-8" standalone="yes"?><virtualmachine><identifier>74</identifier><queueitem><identifier>0</identifier><name>UPGRADE</name><state>REJECTED</state><numberOfFailures>0</numberOfFailures><serviceCode>2bc15b02-e78b-48c0-aa1d-b3c22ece4abe</serviceCode><scope>VirtualMachine</scope><delegatedQueueItems /><startExecution>2014-10-23T09:42:24.258-02:00</startExecution><doPoweroff>false</doPoweroff><doPoweron>false</doPoweron><expectedSteps>0</expectedSteps><currentStep>0</currentStep><message>ValidationException: ServiceCode is not unique</message></queueitem><frequency>900</frequency><memory>2097152</memory><vcpu>2</vcpu></virtualmachine>
            #endregion
        }
    }
}