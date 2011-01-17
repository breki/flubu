using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    public class ImportMachineTask : TaskBase
    {
        private readonly string hostName;
        private string machineName;
        private string importLocation;
        private string machineLocation;

        public ImportMachineTask(string host)
        {
            hostName = host;
        }

        public static ImportMachineTask New(string host, string machineName, string importLocation,
                                            string machineLocation)
        {
            ImportMachineTask task = new ImportMachineTask(host);
            task.Name(machineName)
                .Location(machineLocation)
                .ImportLocation(importLocation);
            return task;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public ImportMachineTask Name(string vmName)
        {
            machineName = vmName;
            return this;
        }

        public ImportMachineTask ImportLocation(string path)
        {
            importLocation = path;
            return this;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public ImportMachineTask Location(string vmLocation)
        {
            machineLocation = vmLocation;
            return this;
        }

        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "Import virtual machine. Host:{0},Name:{1},Location:{2},Import:{3}", hostName,
                                     machineName, machineLocation, importLocation);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            ImportVirtualMachine import = new ImportVirtualMachine(hostName);
            import.ImportVirtualSystem(importLocation, machineName, machineLocation);
        }
    }
}