using System;
using System.Management;
using Utils;

namespace Engine.Cloud.Core.Utils
{
    public static class ChangePasswordWindows
    {

        public static void TryChange(string userName, string ip, string defaultPassword, string newPassword)
        {
            try
            {
                Throw.IfIsTrue(string.IsNullOrEmpty(ip), new Exception("parâmetro ip inválido."));

                ManagementBaseObject outParams = null;

                ManagementScope managementScope = GetManagementScope(userName, ip, defaultPassword);
                outParams = ExecuteCommand(managementScope, userName, newPassword);

                LogFactory.GetInstance().Log(String.Format("Comando de alteração de senha executado com sucesso para o ip  {0} para {1}", ip, newPassword));
            }
            catch (Exception ex)
            {
                var error =
                    string.Format(
                        "Tentando ao alterar senha do servidor windows '{0}', ip '{1}', comando 'wmi', resposta '{2}'",
                        userName, ip, ex.Message);
                LogFactory.GetInstance().Log(error);
                throw;
            }
        }

        private static ManagementScope GetManagementScope(string userName, string ip, string defaultPassword)
        {
            var connectionOptions = new ConnectionOptions();
            connectionOptions.Username = userName;
            connectionOptions.Password = defaultPassword;
            connectionOptions.EnablePrivileges = true;
            connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
            var command = String.Format(@"\\{0}\Root\cimv2", ip);
            var managementScope = new ManagementScope(command, connectionOptions);

            managementScope.Connect();

            return managementScope;
        }

        private static ManagementBaseObject ExecuteCommand(ManagementScope managementScope, string userName, string newPassword)
        {
            var managementPath = new ManagementPath("Win32_Process");
            var processClass = new ManagementClass(managementScope, managementPath, new ObjectGetOptions());
            ManagementBaseObject parameters = processClass.GetMethodParameters("Create");
            var commandLine = String.Format("c:\\Windows\\System32\\net.exe user  {0} \"{1}\" ", userName, newPassword);
            parameters["CommandLine"] = commandLine;

            var outParams = processClass.InvokeMethod("Create", parameters, null);

            return outParams;
        }


    }
}
