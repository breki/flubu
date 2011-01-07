using System;
using System.Collections.Generic;
using System.Text;

namespace Flubu.Tasks.Configuration
{
    /// <summary>
    /// Logs important enviroment information (machine name, OS version, etc).
    /// </summary>
    public class LogScriptEnvironmentTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get { return "Log script environment"; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is safe to execute in dry run mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is safe to execute in dry run mode; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSafeToExecuteInDryRun
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            // log important environment information
            context.WriteInfo("Machine name: {0}", Environment.MachineName);
            context.WriteInfo("OS Version: {0}", Environment.OSVersion);
            context.WriteInfo("User name: {0}", Environment.UserName);
            context.WriteInfo("User domain name: {0}", Environment.UserDomainName);
            context.WriteInfo("CLR version: {0}", Environment.Version);
            context.WriteInfo("Current directory: {0}", Environment.CurrentDirectory);
        }
    }
}
