using System;
using Renci.SshNet;
using Utils;
using Engine.Cloud.Core.Model;

namespace Engine.Cloud.Core.Utils
{
    public static class ConfigureDedicatedFirewallLinux
    {
        public static void TryConfigure(string userName, string ip, string defaultPassword, string typeInstall, string customerCode, string alias, string vlan, string ipFw01, string ipFw02, string ipVip)
        {
            string commandLine = string.Format("./install.sh {0} {1} {2} {3} {4}",
                typeInstall, customerCode, alias, vlan, ipFw01);

            string commandCheck = "if [ -e /tmp/sucesso.txt ]; then echo 1; else echo 0; fi";

            if (Convert.ToInt16(typeInstall) == (int)TypeFirewallInstall.Failover)
            {
                commandLine = string.Format("./install.sh {0} {1} {2} {3} {4} {5} {6}",
                typeInstall, customerCode, alias, vlan, ipFw01, ipFw02, ipVip);
            }
            
            string responseCommand = string.Empty;
            string responseCommandCheck = string.Empty;

            try
            {
                Throw.IfIsTrue(string.IsNullOrEmpty(ip), new Exception("parâmetro ip inválido."));

                using (var client = new SshClient(ip, userName, defaultPassword))
                {
                    client.Connect();
                    responseCommand = client.RunCommand(commandLine).Result;
                    responseCommandCheck = client.RunCommand(commandCheck).Result;
                    client.Disconnect();

                    if (responseCommandCheck.Contains("1"))
                    {
                        LogFactory.GetInstance().Log(String.Format("Comando {0} executado com sucesso. {1}. Ip {2}", commandLine, responseCommand, ip));
                    }
                    else
                    {
                        throw new Exception(String.Format("Erro ao tentar conectar no servidor"));
                    }
                }
            }
            catch (Exception ex)
            {
                var error =
                    string.Format(
                        "Tentando configurar firewall no servidor Linux '{0}', ip '{1}', comando '{2}', resposta '{3}', erro '{4}'",
                        userName, ip, commandLine, responseCommand, ex.Message);
                LogFactory.GetInstance().Log(error);
                throw;
            }
        }

    }
}
