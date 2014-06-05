using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    /// <summary>
    /// Starts virtual machine on hyper-v server.
    /// <code>
    ///         StartMachineTask
    ///            .New(runner.GetValue(targetName, "host"), runner.GetValue(targetName, "machine"))
    ///            .Execute(ScriptExecutionEnvironment);
    /// </code>
    /// </summary>
    public class StartMachineTask : TaskBase
    {
        private readonly string hostName;
        private string machineName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartMachineTask"/> class.
        /// </summary>
        /// <param name="host">Hyper-v host server.</param>
        public StartMachineTask(string host)
        {
            hostName = host;
        }

        /// <summary>
        /// Initializes new instance of <see cref="StartMachineTask"/>
        /// </summary>
        /// <param name="host">Hyper-v host server.</param>
        /// <param name="machineName">Virtual machine name to start.</param>
        /// <returns>New instance of <see cref="StartMachineTask"/></returns>
        public static StartMachineTask New(string host, string machineName)
        {
            StartMachineTask task = new StartMachineTask(host);
            return task.Name(machineName);
        }

        /// <summary>
        ///   Sets virtual machine name to start.
        /// </summary>
        /// <param name = "vmName">Name of the virtual machine.</param>
        /// <returns>This same instance</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public StartMachineTask Name(string vmName)
        {
            machineName = vmName;
            return this;
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Start virtual machine. Host:{0},Machine:{1}",
                    hostName, 
                    machineName);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Flubu.IFlubuLogger.LogMessage(System.String)")]
        protected override void DoExecute(ITaskContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            if (string.IsNullOrEmpty(hostName))
                throw new TaskExecutionException("Host name can not be empty!");
            if (string.IsNullOrEmpty(machineName))
                throw new TaskExecutionException("Machine name can not be empty!");
            using (HyperVManager manager = new HyperVManager())
            {
                manager.Connect(hostName);

                IVirtualTask t = manager.StartVirtualMachine(machineName);

                context.WriteDebug("Waiting for machine to start...");
                t.WaitForCompletion(new TimeSpan(0, 0, 2, 0));
            }
        }
    }
}