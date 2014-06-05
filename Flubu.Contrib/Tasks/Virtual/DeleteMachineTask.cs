using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    public class DeleteMachineTask : TaskBase
    {
        public DeleteMachineTask(string host)
        {
            hostName = host;
        }

        public static DeleteMachineTask New(string host, string machineName)
        {
            DeleteMachineTask task = new DeleteMachineTask(host);
            return task.Name(machineName);
        }

        public DeleteMachineTask FailIfNotExists(bool fail)
        {
            failIfNotExists = fail;
            return this;
        }

        /// <summary>
        ///   Set virtual machine name.
        /// </summary>
        /// <param name = "vmName">Name of the virtual machine.</param>
        /// <returns>This same instance of <see cref="DeleteMachineTask"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public DeleteMachineTask Name(string vmName)
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
            try
            {
                using (HyperVManager manager = new HyperVManager())
                {
                    manager.Connect(hostName);

                    IVirtualTask t = manager.DeleteVirtualMachine(machineName);

                    t.WaitForCompletion(new TimeSpan(0, 0, 0, 10));
                }
            }
            catch (Exception e)
            {
                if (failIfNotExists)
                    throw new TaskExecutionException("Error deleting virtual machine", e);

                context.WriteDebug("Virtual machine does not exist and was not deleted!");
            }
        }

        private readonly string hostName;
        private string machineName;
        private bool failIfNotExists = true;
    }
}