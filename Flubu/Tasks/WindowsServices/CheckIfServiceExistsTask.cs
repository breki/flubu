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
            this.machineName = ".";
        }

        public CheckIfServiceExistsTask(string machineName, string serviceName, string configurationSetting)
        {
            this.serviceName = serviceName;
            this.configurationSetting = configurationSetting;
            this.machineName = machineName;
        }

        public static void Execute (
            ITaskContext environment,
            string serviceName, 
            string configurationSetting)
        {
            CheckIfServiceExistsTask task = new CheckIfServiceExistsTask (serviceName, configurationSetting);
            task.Execute (environment);
        }

        public static void Execute(
    ITaskContext environment,
    string machineName,
    string serviceName,
    string configurationSetting)
        {
            CheckIfServiceExistsTask task = new CheckIfServiceExistsTask(machineName, serviceName, configurationSetting);
            task.Execute(environment);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "serviceHandle")]
        protected override void DoExecute (ITaskContext context)
        {
            try
            {
                using (ServiceController serviceController = new ServiceController (serviceName, machineName))
                {
                    // this should throw an exception if the service does not exist
                    System.Runtime.InteropServices.SafeHandle serviceHandle = serviceController.ServiceHandle;
                    context.Properties.Set (configurationSetting, "true");
                    context.WriteInfo("Windows service '{0}:{1}' exists.", machineName, serviceName);
                }
            }
            catch (InvalidOperationException e)
            {
                context.Properties.Set(configurationSetting, false);
                context.WriteInfo(
                    "Windows service '{0}' does not exist.{1}", 
                    serviceName,
                    e.Message);
            }
        }

        private string serviceName;
        private string configurationSetting;
        private string machineName;
    }
}
