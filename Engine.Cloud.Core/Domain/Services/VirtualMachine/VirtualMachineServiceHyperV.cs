using System;
using System.Data.Services.Client;
using System.Linq;
using Engine.Cloud.Core.Domain.Services.VirtualMachine.Mappers;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.Results;
using Utils;
using System.Collections.Generic;
using Engine.Cloud.Core.Utils.Logging;
using System.Net.Http;
using Newtonsoft.Json;

namespace Engine.Cloud.Core.Domain.Services.VirtualMachine
{
    public class VirtualMachineServiceHyperV : HyperVServiceBase, IVirtualMachineService
    {
        public DataServiceQuery<ServiceReferenceVmm.VirtualMachine> GetListVirtualMachines()
        {
            try
            {
                return ContextVmm.VirtualMachines;
            }
            catch (Exception ex)
            {
                throw new VirtualMachineServiceException("erro ao carregar dados do hypervisor", ex);
            }
        }

        public void Load(Server server)
        {
            try
            {
                new MapperVirtualMachineServiceHyperV(base.ContextVmm).MapperToServer(server);
            }
            catch (Exception ex)
            {
                throw new VirtualMachineServiceException("erro ao carregar dados do hypervisor", ex);
            }
        }

        public Server Load(string hypervisorIdentifier)
        {
            try
            {
                var server = new Server() { HypervisorIdentifier = hypervisorIdentifier };
                new MapperVirtualMachineServiceHyperV(base.ContextVmm).MapperToServer(server);
                return server;
            }
            catch (Exception ex)
            {
                throw new VirtualMachineServiceException("erro ao carregar dados do hypervisor", ex);
            }
        }

        public void LoadAll(List<Server> servers)
        {
            throw new System.NotImplementedException();
        }

        public ServiceResult Install(Client client, byte vcpu, int frequency, int memory, string image, int diskSize, decimal[] bandwidths, int partitions, string formatDisk, long serverId)
        {
            try
            {
                var template = base.ContextVmm.VMTemplates.Where(x => x.Name == image).First();

                var virtualMachine = new ServiceReferenceVmm.VirtualMachine();

                virtualMachine.Name = string.Format("{0}.{1}", client.CustomerCode.Replace(".", ""), serverId);
                virtualMachine.VMHostName = string.Format("{0}{1}", client.CustomerCode.Replace(".", ""), serverId);
                virtualMachine.Description = string.Format("{0}.{1}", client.CustomerCode.Replace(".", ""), serverId);

                virtualMachine.StampId = base.StampVmm.ID; //vmm da vez
                virtualMachine.CloudId = base.CloudOnVmm.ID; //cloud da vez

                virtualMachine.CPUCount = vcpu;
                virtualMachine.Memory = (memory / 1024);


                virtualMachine.VMTemplateId = template.ID;

                base.ContextVmm.AddToVirtualMachines(virtualMachine);

                DataServiceResponse result = ContextVmm.SaveChanges();

                return ParseResultSPF(result);
            }
            catch (Exception ex)
            {
                LogFactory.GetInstance().Log(ex);
                return new ServiceResult { Result = ex.Message, Status = StatusService.FAILED };
            }
        }

        public ServiceResult Uninstall(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Tag = "UNINSTALLED";

            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Reinstall(Server server, Image image)
        {
            var urlBase = string.Format("{0}/api/virtualmachine/reinstall?id={1}&templateName={2}", UrlApiVmm, server.HypervisorIdentifier, image.Name);

            var postData = new List<KeyValuePair<string, string>>();
            HttpResponseMessage httpResponseMessage;
            _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), urlBase, postData));

            string serviceCode;

            using (var httpClient = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(postData);
                httpClient.BaseAddress = new Uri(urlBase);
                httpResponseMessage = httpClient.PostAsync(urlBase, content).Result;
                httpResponseMessage.EnsureSuccessStatusCode();

                dynamic response = JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result);
                // algo assim : {"Version":{"_Major":1,"_Minor":1,"_Build":-1,"_Revision":-1},"Content":null,"StatusCode":201,"ReasonPhrase":"Created","Headers":[{"Key":"X-VMMJobID","Value":["906cddce-2542-4dee-80b4-d75f32d72b80"]}],"RequestMessage":null,"IsSuccessStatusCode":true}
                serviceCode = response.Headers[0].Value[0]; // pegando X-VMMJobID
            }
            return new ServiceResult { Result = serviceCode, Status = StatusService.SUCCESS };
        }

        public ServiceResult PowerOn(Server server)
        {
            this.Refresh(server);

            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();

            vm.Operation = "Start";
            base.ContextVmm.UpdateObject(vm);

            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult ShutDown(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Shutdown";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();
            return ParseResultSPF(result);
        }

        public ServiceResult PowerOff(Server server)
        {
            string url = string.Empty;
            string serviceCode = string.Empty;

            try
            {
                url = string.Format("{0}/api/virtualmachine/status/{1}?status=stop-force", UrlApiVmm, server.HypervisorIdentifier);
                var putData = new List<KeyValuePair<string, string>>();
                _logger.Log(string.Format("{0}, request: {1}, body: {2}", LogUtils.GetCurrentMethod(this), url, putData));
                using (var httpClient = new HttpClient())
                {
                    HttpContent content = new FormUrlEncodedContent(putData);
                    httpClient.BaseAddress = new Uri(url);
                    HttpResponseMessage httpResponseMessage = httpClient.PostAsync(url, content).Result;
                    httpResponseMessage.EnsureSuccessStatusCode();

                    dynamic response = JsonConvert.DeserializeObject(httpResponseMessage.Content.ReadAsStringAsync().Result);
                    // algo assim : {"Version":{"_Major":1,"_Minor":1,"_Build":-1,"_Revision":-1},"Content":null,"StatusCode":201,"ReasonPhrase":"Created","Headers":[{"Key":"X-VMMJobID","Value":["906cddce-2542-4dee-80b4-d75f32d72b80"]}],"RequestMessage":null,"IsSuccessStatusCode":true}
                    serviceCode = response.Headers[0].Value[0]; // pegando X-VMMJobID
                }
            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("{0}, url: {1}", LogUtils.GetCurrentMethod(this), url), ex);
                return new ServiceResult { Result = server.HypervisorIdentifier, Status = StatusService.FAILED };
            }
            return new ServiceResult { Result = serviceCode, Status = StatusService.SUCCESS };
        }

        private static ServiceResult ParseResultSPF(DataServiceResponse result)
        {
            OperationResponse vmmJob = result.Where(x => x.StatusCode == 204 || x.StatusCode == 201).FirstOrDefault();

            if (vmmJob == null)
                return new ServiceResult() { Result = string.Empty, Status = StatusService.FAILED };

            var serviceCode = vmmJob.Headers["x-ms-request-id"];

            return new ServiceResult() { Result = serviceCode, Status = StatusService.SUCCESS };
        }

        public ServiceResult Reboot(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Shutdown";
            base.ContextVmm.UpdateObject(vm);
            base.ContextVmm.SaveChanges();

            vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Start";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Suspend(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Suspend";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Unlock(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Resume";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Resume(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Resume";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Reset(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Reset";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Pause(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Stop";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }

        public ServiceResult Refresh(Server server)
        {
            var vm = base.ContextVmm.VirtualMachines.Where(x => x.ID == Guid.Parse(server.HypervisorIdentifier)).First();
            vm.Operation = "Refresh";
            base.ContextVmm.UpdateObject(vm);
            DataServiceResponse result = ContextVmm.SaveChanges();

            return ParseResultSPF(result);
        }
    }
}