using System;
using System.Collections.Generic;
using System.Text;

namespace Flubu.Tasks.WindowsServices
{
    /// <summary>
    /// Stops the specified Windows service if it exists on the system.
    /// </summary>
    public class StopWindowsServiceIfExistsTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Stop Windows service '{0}' if it exists.", 
                    serviceName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StopWindowsServiceIfExistsTask"/> class
        /// with the specified service name.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public StopWindowsServiceIfExistsTask (string serviceName)
        {
            this.serviceName = serviceName;
        }

        public static void Execute (
            ITaskContext environment,
            string serviceName)
        {
            StopWindowsServiceIfExistsTask task = new StopWindowsServiceIfExistsTask (serviceName);
            task.Execute (environment);
        }

        /// <summary>
        /// Method defining the actual work for a task.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            string configSettingName = String.Format (
                System.Globalization.CultureInfo.InvariantCulture,
                "ServicesExist/{0}", 
                serviceName);
            CheckIfServiceExistsTask.Execute (context, serviceName, configSettingName);
            if (context.Properties.Get<bool>(configSettingName))
                ControlWindowsServiceTask.Execute (
                    context, 
                    serviceName,
                    ControlWindowsServiceMode.Stop, 
                    TimeSpan.FromSeconds (30));
        }

        private string serviceName;
    }
}
