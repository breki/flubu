using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    public class ShutdownMachineTask : TaskBase
    {
        private readonly string hostName;
        private string machineName;
        private bool failIfNotExists = true;

        public ShutdownMachineTask(string host)
        {
            hostName = host;
        }

        public static ShutdownMachineTask New(string host, string machineName)
        {
            ShutdownMachineTask task = new ShutdownMachineTask(host);
            return task.Name(machineName);
        }

        public ShutdownMachineTask FailIfNotExists(bool fail)
        {
            failIfNotExists = fail;
            return this;
        }


        /// <summary>
        ///   Set virtual machine name.
        /// </summary>
        /// <param name = "vmName">Name of the virtual machine.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public ShutdownMachineTask Name(string vmName)
        {
            machineName = vmName;
            return this;
        }

        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "Shutdown virtual machine. Host:{0},Machine:{1}",
                                     hostName, machineName);
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
            try
            {
                using (HyperVManager manager = new HyperVManager())
                {
                    manager.Connect(hostName);

                    IVirtualTask t = manager.ShutdownVirtualMachine(machineName);

                    context.WriteDebug("Waiting for machine to shutdown...");
                    t.WaitForCompletion(new TimeSpan(0, 0, 2, 0));
                }
            }
            catch (Exception e)
            {
                if (failIfNotExists)
                    throw new TaskExecutionException("Virtual machine shutdown failed", e);

                context.WriteDebug("Virtual machine does not exist and was not shutdown!");
            }
        }
    }
}