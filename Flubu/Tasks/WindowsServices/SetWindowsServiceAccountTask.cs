using System;
using System.Management;

namespace Flubu.Tasks.WindowsServices
{
    public class SetWindowsServiceAccountTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Set Windows service '{0}' account to '{1}'.", 
                    serviceName, 
                    userName);
            }
        }

        public SetWindowsServiceAccountTask (string serviceName, string userName, string password)
        {
            this.serviceName = serviceName;
            this.userName = userName;
            this.password = password;
        }

        public static void Execute (
            ITaskContext environment,
            string serviceName, 
            string userName, 
            string password)
        {
            SetWindowsServiceAccountTask task = new SetWindowsServiceAccountTask (serviceName, userName, password);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            string objPath = string.Format (
                System.Globalization.CultureInfo.InvariantCulture,
                "Win32_Service.Name='{0}'", 
                serviceName);
            using (ManagementObject service = new ManagementObject (new ManagementPath (objPath)))
            {
                object[] wmiParams = new object[11];
                wmiParams[6] = userName;
                wmiParams[7] = password;
                service.InvokeMethod ("Change", wmiParams);
            }
        }

        private string serviceName;
        private string userName;
        private string password;
    }
}
