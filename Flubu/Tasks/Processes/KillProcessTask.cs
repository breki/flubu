using System;
using System.Diagnostics;

namespace Flubu.Tasks.Processes
{
    /// <summary>
    /// Kills a specified process.
    /// </summary>
    public class KillProcessTask : TaskBase
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
                    "Kill process '{0}'", 
                    processName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KillProcessTask"/> class using a specified process name.
        /// </summary>
        /// <param name="processName">Name of the process to be killed.</param>
        public KillProcessTask (string processName)
        {
            this.processName = processName;
        }

        public static void Execute (ITaskContext environment, string processName)
        {
            KillProcessTask task = new KillProcessTask (processName);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            Process[] processByName = Process.GetProcessesByName (processName);
            foreach (Process process in processByName)
            {
                context.WriteInfo("Killing process '{0}'", process.ProcessName);
                process.Kill ();
            }
        }

        private string processName;
    }
}
