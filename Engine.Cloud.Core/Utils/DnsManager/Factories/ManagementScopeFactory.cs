using System;
using System.Management;

namespace Engine.Cloud.Core.Utils.DnsManager.Factories
{
    public static class ManagementScopeFactory
    {
        private static ManagementScope Create(string serverName, string userName, string password)
        {
            try
            {
                var connectionOptions = new ConnectionOptions();
                connectionOptions.Username = userName;
                connectionOptions.Password = password;

                connectionOptions.EnablePrivileges = true;
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                connectionOptions.Timeout = new TimeSpan(1, 0, 0);

                ManagementScope scope = new ManagementScope(String.Format(@"\\{0}\Root\MicrosoftDNS", serverName), connectionOptions);
                scope.Connect();
                return scope;
            }
            catch (Exception ex)
            {
                var log = string.Format("serverName {0}, userName {1}", serverName, userName);
                throw new Exception(log, ex);
            }
        }

        public static ManagementScope Create(ConfigurationBuilder configuration)
        {
            return Create(configuration.IP, configuration.User, configuration.Pass);
        }
    }
}
