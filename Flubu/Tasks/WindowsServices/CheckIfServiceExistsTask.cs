using System;
using System.ServiceProcess;

namespace Flubu.Tasks.WindowsServices
{
    public class CheckIfServiceExistsTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Check if Windows service '{0}' exists.", 
                    serviceName);
            }
        }

        public CheckIfServiceExistsTask (string serviceName, string configurationSetting)
        {
            this.serviceName = serviceName;
            this.configurationSetting = configurationSetting;
        }

        public static void Execute (
            ITaskContext environment,
            string serviceName, 
            string configurationSetting)
        {
            CheckIfServiceExistsTask task = new CheckIfServiceExistsTask (serviceName, configurationSetting);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            try
            {
                using (ServiceController serviceController = new ServiceController (serviceName))
                {
                    // this should throw an exception if the service does not exist
                    System.Runtime.InteropServices.SafeHandle serviceHandle = serviceController.ServiceHandle;
                    context.Properties.Set (configurationSetting, "true");
                    context.WriteInfo("Windows service '{0}' exists.", serviceName);
                }
            }
            catch (InvalidOperationException)
            {
                context.Properties.Set(configurationSetting, false);
                context.WriteInfo(
                    "Windows service '{0}' does not exist.", 
                    serviceName);
            }
        }

        private string serviceName;
        private string configurationSetting;
    }
}
