using System;
using System.Management;

namespace RemoteServiceRestart
{
    public class ServiceController
    {
        private ManagementScope Scope;

        private string UserName;
        private string Password;
        private string ServerName;
        private string ServiceName;

        private string ServiceStatusQuery = "SELECT State FROM Win32_Service WHERE Name LIKE";

        public ServiceController(string serverName, string userName, string password, string serviceName)
        {
            ServerName = serverName;
            UserName = userName;
            Password = password;
            ServiceName = serviceName;
        }

        public void Connect()
        {
            try
            {
                ConnectionOptions co = new ConnectionOptions
                {
                    Username = UserName,
                    Password = Password,
                    Timeout = new TimeSpan(0, 2, 0)
                };

                Scope = null;
                Scope = new ManagementScope(GetWmiPathOs(ServerName), co);
                Scope.Connect();
            }
            catch (Exception ex)
            {
                throw new Exception("Error making WMI connection to server." + Environment.NewLine + ex.Message);
            }
        }

        public void StopService()
        {
            InvokeMethodOnService("StopService");
        }

        public void StartService()
        {
            InvokeMethodOnService("StartService");
        }

        private void InvokeMethodOnService(string methodToInvoke)
        {
            SelectQuery query = new SelectQuery("select * from Win32_Service where name = '" + ServiceName + "'");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(Scope, query))
            {
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject service in collection)
                {
                    service.InvokeMethod(methodToInvoke, null);
                }
            }
        }

        public void WaitForServiceToStop()
        {
            WaitForServiceState("Stopped");
        }

        public void WaitForServiceToBeRunning()
        {
            WaitForServiceState("Running");
        }

        private void WaitForServiceState(string desiredState)
        {
            int i = 0;
            int seconds = 5;
            int loopMax = 10;
            bool run = true;

            do
            {
                i++;

                if (GetServiceStatus().Equals(desiredState))
                {
                    return;
                }

                if (i > loopMax)
                {
                    run = false;
                    throw new Exception("Service unable to reach desired state (" + desiredState + ") after " + seconds * loopMax + " seconds.");
                }

                System.Threading.Thread.Sleep(1000 * seconds);
            } while (run);
        }

        public string GetServiceStatus()
        {
            string query = ServiceStatusQuery + " '" + ServiceName + "%'";
            string state = "";

            using (var searcher = new ManagementObjectSearcher(Scope, new SelectQuery(query)))
            {
                searcher.Options.Timeout = new TimeSpan(0, 3, 0);

                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject mobj in collection)
                {
                    foreach (PropertyData prop in mobj.Properties)
                    {
                        if (prop != null)
                            if (!string.IsNullOrEmpty(prop.Name.ToString()) && !string.IsNullOrEmpty(prop.Value.ToString()))
                                state += prop.Value.ToString();
                    }
                }
            }

            return state;
        }

        private string GetWmiPathOs(string serverName)
        {
            return string.Format(@"\\{0}\root\cimv2", serverName);
        }
    }
}