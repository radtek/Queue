using System;
using System.Linq;
using System.Threading;
using Engine.Cloud.Core.Model;
using Engine.Cloud.Core.Model.DataContext;
using Engine.Cloud.Core.Model.Results;
using Engine.Cloud.Core.Utils;
using Engine.Cloud.Core.Utils.Extensions;
using Engine.Cloud.Core.Utils.Logging;
using Polly;
using Utils;

namespace Engine.Cloud.Core.Domain
{
    public class SendInstructionsDomain
    {
        private readonly ClientDomain _clientDomain;
        private readonly ServerDomain _serverDomain;
        private readonly ILogger _logger;
        private readonly string _to;

        public SendInstructionsDomain(EngineCloudDataContext context)
        {
            _clientDomain = new ClientDomain(context);
            _serverDomain = new ServerDomain(context);
            _logger = LogFactory.GetInstance();
            _to = AppSettings.GetString("EmailError.To");
        }

        public QueueActionResult SendDeleteServerInstructions(Server server)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(server.Client.CustomerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}",
                        server.Client.CustomerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", server.Client.CustomerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Exclusão de servidor Cloud ENGINE.";

                sendEmailClient.Template = @"\Templates\pt-br\Email\DeleteServerInstruction.html";

                sendEmailClient.Params.Add("##CustomerCode##", server.Client.CustomerCode);
                sendEmailClient.Params.Add("##ServerName##", server.Name);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendChangeConfigurationServerInstructions(Server server, string operation)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(server.Client.CustomerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}",
                         server.Client.CustomerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", server.Client.CustomerCode));
                    sendEmailClient.To = _to;
                }

                _serverDomain.Load(server);

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Alteração de servidor Cloud ENGINE.";

                sendEmailClient.Template = @"\Templates\pt-br\Email\ChangeServerConfiguration.html";

                sendEmailClient.Params.Add("##Operation##", operation);

                sendEmailClient.Params.Add("##CustomerCode##", server.Client.CustomerCode);
                sendEmailClient.Params.Add("##ServerName##", server.Name);

                sendEmailClient.Params.Add("##IPServer##", server.Resources.NetworkInterfaces[0].Ips.Concatenate(x => x.Number, ", "));
                sendEmailClient.Params.Add("##Memory##", (server.Resources.Memory / 1024).ToString());

                sendEmailClient.Params.Add("##Processing##", string.Format("{0} x {1}", server.Resources.Vcpu, server.Resources.Frequency));

                
                sendEmailClient.Params.Add("##Bandwith##", server.Resources.NetworkInterfaces.Sum(x => x.BandWidth).ToString());

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendInstallServerInstructions(Server server)
        {
            try
            {
                _serverDomain.Load(server);

                new Thread(() =>
                {
                    try
                    {
                        Thread.CurrentThread.IsBackground = true;

                        SendInstallServerInstructionsTask(server);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(string.Format("erro ao enviar email de instruções para o servidor {0}", server.Name), ex);
                    }

                }).Start();

            }
            catch (Exception ex)
            {
                _logger.Log(string.Format("erro ao tentar enviar (load) email de instruções {0}", server.Name), ex);
                return new QueueActionResult { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public void SendAdditionalUserRecoverPasswordInstructions(string user, string newPassword, string email)
        {
            var sendEmailClient = new SendEmailClient();
            sendEmailClient.To = email;

            sendEmailClient.Subject = "[Engine] Acesso ao seu Painel de Controle.";
            sendEmailClient.Template = @"\bin\Templates\pt-br\Email\AdditionalUser_SendRecoverPasswordInstructions.html";
            sendEmailClient.Params.Add("##User##", user);
            sendEmailClient.Params.Add("##Password##", newPassword);

            sendEmailClient.Send();
            
            _logger.Log(string.Format("Senha de '{0} - ({1})' enviada para '{2}'.", user, newPassword, sendEmailClient.To));
        }

        private void SendInstallServerInstructionsTask(Server server)
        {
            var sendEmailClient = new SendEmailClient();

            string password = null;

            sendEmailClient.To = _clientDomain.GetEmailCustomer(server.Client.CustomerCode);

            if (string.IsNullOrWhiteSpace(sendEmailClient.To))
            {
                var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}",
                    server.Client.CustomerCode);

                sendEmailClient.Params.Add("##Error##", msg);

                Throw.IfIsNull(_to, new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", server.Client.CustomerCode)));

                sendEmailClient.To = _to;
            }

            if (!sendEmailClient.Params.ContainsKey("##Error##"))
                sendEmailClient.Params.Add("##Error##", " ");

            sendEmailClient.Subject = "[ENGINE] Dados de acesso ao servidor Cloud ENGINE.";

            sendEmailClient.Template = @"\Templates\pt-br\Email\InstallServerInstruction.html";

            sendEmailClient.Params.Add("##CustomerCode##", server.Client.CustomerCode);
            sendEmailClient.Params.Add("##ServerName##", server.Name);
            sendEmailClient.Params.Add("##UserName##",
                server.Image.Name.Contains("windows") ? "Administrator" : "root");

            sendEmailClient.Params.Add("##IPServer##",
                server.Resources.NetworkInterfaces[0].Ips.Concatenate(x => x.Number, ", "));
            sendEmailClient.Params.Add("##Memory##", (server.Resources.Memory / 1024).ToString());

            sendEmailClient.Params.Add("##Processing##",
                string.Format("{0} x {1}", server.Resources.Vcpu, server.Resources.Frequency));

         
            sendEmailClient.Params.Add("##Bandwith##",
                server.Resources.NetworkInterfaces.Sum(x => x.BandWidth).ToString());

            var ip = server.Resources.NetworkInterfaces[0].Ips[0].Number;
            var plataform = server.Image.Plataform;

            if (server.Image.TypeHipervisorId == (int)TypeHypervisor.XEN_RJ1 ||
                server.Image.TypeHipervisorId == (int)TypeHypervisor.XEN_SP1)
            {
                password = TryChangePassword(server);
                sendEmailClient.Params.Add("##Password##", password);
            }
            else
            {
                sendEmailClient.Params.Add("##Password##", "Definir no primeiro logon via console.");
            }

            sendEmailClient.Send();

            _logger.Log(string.Format("Senha de '{0} - ({1}) - ({3})' enviada para '{2}'.", ip, plataform, sendEmailClient.To, password));
        }

     

        private string TryChangePassword(Server server)
        {
            const int LIMIT = 4;
            string newPassword = IntExtentions.GetRandomNumber(7);
            string defaultPassword = server.Image.Password;

            try
            {
                if (server.Image.Name.Contains("windows"))
                {
                    Policy
                        .Handle<Exception>()
                        .WaitAndRetry(LIMIT, x => TimeSpan.FromSeconds(Math.Pow(3, x)))
                        .Execute(
                            () =>
                                ChangePasswordWindows.TryChange("Administrator",
                                    server.Resources.NetworkInterfaces[0].Ips[0].Number, server.Image.Password,
                                    newPassword));
                }
                else
                {
                    Policy
                        .Handle<Exception>()
                        .WaitAndRetry(LIMIT, x => TimeSpan.FromSeconds(Math.Pow(3, x)))
                        .Execute(
                            () => ChangePasswordLinux.TryChange("root",
                                server.Resources.NetworkInterfaces[0].Ips[0].Number, server.Image.Password,
                                newPassword));
                }

                return newPassword;
            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                return defaultPassword;
            }
        }

        public QueueActionResult SendCreateLoadBalanceInstructions(string customerCode, string identifier, string bandWidth)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Criação de Load Balance no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\LoadBalance_SendCreateInstructions.html";

                sendEmailClient.Params.Add("##Account##", identifier);
                sendEmailClient.Params.Add("##BandWidth##", bandWidth);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendChangeLoadBalanceInstructions(string customerCode, string identifier, string bandWidth)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Alteração de Banda Load Balance no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\LoadBalance_SendChangeInstructions.html";

                sendEmailClient.Params.Add("##Account##", identifier);
                sendEmailClient.Params.Add("##BandWidth##", bandWidth);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendCancelLoadBalanceInstructions(string customerCode, string identifier)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Cancelamento de Load Balance no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\LoadBalance_SendCancelInstructions.html";

                sendEmailClient.Params.Add("##Account##", identifier);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendCreatePrivateImageInstructions(string customerCode, string identifier, string diskSize)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Criação de Imagem de Usuário no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\PrivateImage_SendCreateInstructions.html";

                sendEmailClient.Params.Add("##Account##", identifier);
                sendEmailClient.Params.Add("##DiskSize##", diskSize);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendChangePrivateImageInstructions(string customerCode, string identifier, string diskSize)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Alteração de Imagem de Usuário no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\PrivateImage_SendChangeInstructions.html";

                sendEmailClient.Params.Add("##Account##", identifier);
                sendEmailClient.Params.Add("##DiskSize##", diskSize);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendCancelPrivateImageInstructions(string customerCode, string identifier)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Cancelamento de Imagem Privada no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\PrivateImage_SendCancelInstructions.html";

                sendEmailClient.Params.Add("##Account##", identifier);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

    
        public QueueActionResult SendAccountMailManagerInstructions(string customerCode, string account, string passcode, string produto)        
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Dados de acesso ao plano de Email";
                sendEmailClient.Template = @"\Templates\pt-br\Email\MailManager_SendCreateAccountInstructions.html";

                sendEmailClient.Params.Add("##Account##", account);
                sendEmailClient.Params.Add("##Passcode##", passcode);
          
                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendProductMailManagerInstructions(string customerCode, string produto, string quantity, string mensalValue)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Criação de novo plano de Email.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\MailManager_SendCreateProductInstructions.html";

                sendEmailClient.Params.Add("##Product##", produto);
                sendEmailClient.Params.Add("##Quantity##", quantity);
                sendEmailClient.Params.Add("##MensalValue##", mensalValue);

                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }

            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendCreatePrivateVlanInstructions(string customerCode, string name)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);

                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Criação de Rede Privada no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\PrivateVlan_SendCreateInstructions.html";
                sendEmailClient.Params.Add("##Client##", customerCode);
                sendEmailClient.Params.Add("##Name##", name);
                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }

        public QueueActionResult SendCancelPrivateVlanInstructions(string customerCode, string name)
        {
            try
            {
                var sendEmailClient = new SendEmailClient
                {
                    To = _clientDomain.GetEmailCustomer(customerCode)
                };

                if (string.IsNullOrWhiteSpace(sendEmailClient.To))
                {
                    var msg = string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode);
                    sendEmailClient.Params.Add("##Error##", msg);
                    if (string.IsNullOrWhiteSpace(_to))
                        throw new Exception(string.Format("Não foi possível localizar o destinatário para o cliente: {0}", customerCode));
                    sendEmailClient.To = _to;
                }

                if (!sendEmailClient.Params.ContainsKey("##Error##"))
                    sendEmailClient.Params.Add("##Error##", " ");

                sendEmailClient.Subject = "[ENGINE] Cancelamento de rede Privada no Cloud ENGINE.";
                sendEmailClient.Template = @"\Templates\pt-br\Email\PrivateVlan_SendCancelInstructions.html";
                sendEmailClient.Params.Add("##Client##", customerCode);
                sendEmailClient.Params.Add("##Name##", name);
                sendEmailClient.Send();
            }
            catch (Exception ex)
            {
                _logger.Log(LogUtils.GetCurrentMethod(this), ex);
                return new QueueActionResult() { Result = ex.Message, Status = StatusQueueAction.FAILED };
            }
            return new QueueActionResult { Result = "COMPLETED", Status = StatusQueueAction.COMPLETED };
        }
    }
}
