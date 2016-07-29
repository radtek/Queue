using System;
using System.Configuration;
using System.Xml;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;

namespace Engine.Cloud.Core.Domain.Services
{
    public class XenServiceBase
    {
        public int HTTP_TIMEOUT = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Cloud.HTTP.TIMEOUT"));

        public ServiceResult ParseXmlResult(XmlDocument xmlDocument)
        {
            var serviceResult = new ServiceResult();
            var result = GetQueueItem_State(xmlDocument);

            if (result.Contains("FAILED_"))
                result = "FAILED";

            switch (result)
            {
                case "":
                case "FAILED":
                case "REJECTED":
                    serviceResult.Status = StatusService.FAILED;
                    break;
                default:
                    serviceResult.Status = StatusService.SUCCESS;
                    break;
            }

            if (serviceResult.Status == StatusService.FAILED)
            {
                var queueItemServiceCode = GetQueueItemServiceCode(xmlDocument);
                serviceResult.Result = string.Format("{0}: {{{1}}}", result, queueItemServiceCode);
            }
            else
                serviceResult.Result = GetQueueItemServiceCode(xmlDocument);

            return serviceResult;
        }

        public ServiceResult ParseQueueitemNetworkInterfaceState(XmlDocument xmlDocument)
        {
            var serviceResult = new ServiceResult();

            var selectSingleNode = xmlDocument.SelectSingleNode("/networkinterface/queueitem/state").InnerText;

            if (selectSingleNode.StartsWith("FAILED"))
                selectSingleNode = "FAILED";

            switch (selectSingleNode)
            {
                case "COMPLETED":
                    serviceResult.Status = StatusService.SUCCESS;
                    break;
                default:
                    serviceResult.Status = StatusService.FAILED;
                    break;
            }

            serviceResult.Result = xmlDocument.SelectSingleNode("/networkinterface/queueitem/state").InnerText;

            return serviceResult;
        }

        public ServiceResult ParseQueueitemVirtualMachineState(XmlDocument xmlDocument)
        {
            var serviceResult = new ServiceResult();

            var selectSingleNode = xmlDocument.SelectSingleNode("/virtualmachine/queueitem/state").InnerText;

            if (selectSingleNode.StartsWith("FAILED"))
                selectSingleNode = "FAILED";

            switch (selectSingleNode)
            {
                case "COMPLETED":
                    serviceResult.Status = StatusService.SUCCESS;
                    break;
                default:
                    serviceResult.Status = StatusService.FAILED;
                    break;
            }

            serviceResult.Result = xmlDocument.SelectSingleNode("/virtualmachine/queueitem/state").InnerText;

            return serviceResult;
        }

        public string GetQueueItem_State(XmlDocument xmlDocument)
        {
            XmlNode selectSingleNode = null;
            if (xmlDocument.SelectSingleNode("/disk/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/disk/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/virtualmachine/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/virtualmachine/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/networkinterface/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/networkinterface/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/loadBalanceGroup/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/loadBalanceGroup/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/software/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/software/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/userip/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/userip/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/user/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/user/queueitem/state");
            }
            else if (xmlDocument.SelectSingleNode("/vlan/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/vlan/queueitem/state");
            }
            return selectSingleNode != null ? selectSingleNode.InnerText : "";
        }

        public string GetQueueItemServiceCode(XmlDocument xmlDocument)
        {
           
            XmlNode selectSingleNode = null;
            if (xmlDocument.SelectSingleNode("/disk/queueitem/serviceCode") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/disk/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/virtualmachine/queueitem/serviceCode") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/virtualmachine/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/networkinterface/queueitem/serviceCode") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/networkinterface/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/user/queueitem/serviceCode") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/user/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/loadBalanceGroup/name") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/loadBalanceGroup/name");
            }
            else if (xmlDocument.SelectSingleNode("/software/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/software/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/userip/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/userip/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/user/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/user/queueitem/serviceCode");
            }
            else if (xmlDocument.SelectSingleNode("/vlan/queueitem/state") != null)
            {
                selectSingleNode = xmlDocument.SelectSingleNode("/vlan/queueitem/serviceCode");
            }
            return selectSingleNode != null ? selectSingleNode.InnerText : "";
        }

        public string GetQueueItemServerName(XmlDocument xmlDocument)
        {
            var selectSingleNode = xmlDocument.SelectSingleNode("/virtualmachine/name");
            return selectSingleNode != null ? selectSingleNode.InnerText : "";
        }

        public string GetQueueItemHypervisorIdentifier(XmlDocument xmlDocument)
        {
            var selectSingleNode = xmlDocument.SelectSingleNode("/virtualmachine/identifier");
            return selectSingleNode != null ? selectSingleNode.InnerText : "";
        }

        public string GetQueueItemIp(XmlDocument xmlDocument)
        {
            var selectSingleNode = xmlDocument.SelectSingleNode("/virtualmachine/networkinterfaces/networkinterface/ipv4");
            return selectSingleNode != null ? selectSingleNode.InnerText : "";
        }
    }
}