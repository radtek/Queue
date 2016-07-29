using System;
using Renci.SshNet;
using Utils;

namespace Engine.Cloud.Core.Utils
{
    public static class ChangePasswordLinux
    {
        public static void TryChange(string userName, string ip, string defaultPassword, string newPassword)
        {
            string commandLine = String.Format("/root/changepw.sh {0}", newPassword);
            string responseCommand = string.Empty;

            try
            {
                Throw.IfIsTrue(string.IsNullOrEmpty(ip), new Exception("parâmetro ip inválido."));

                using (var client = new SshClient(ip, userName, defaultPassword))
                {
                    client.Connect();
                    responseCommand = client.RunCommand(commandLine).Result;
                    client.Disconnect();

                    if (responseCommand.Contains("OK"))
                    {
                        LogFactory.GetInstance().Log(String.Format("Comando {0} executado com sucesso. {1}. Ip {2}", commandLine, responseCommand, ip));
                    }
                    else
                    {
                        throw new Exception(String.Format("Erro ao tentar conectar no servidor"));
                    }
                }
            }
            catch (Exception)
            {
                var error =
                    string.Format(
                        "Tentando ao alterar senha do servidor Linux '{0}', ip '{1}', comando '{2}', resposta '{3}'",
                        userName, ip, commandLine, responseCommand);
                LogFactory.GetInstance().Log(error);
                throw;
            }
        }

    }
}
