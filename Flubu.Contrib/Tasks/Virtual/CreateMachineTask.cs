using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    /// <summary>
    ///   Create new hyper-v virtual machine on specified server.
    /// </summary>
    public class CreateMachineTask : TaskBase
    {
        private readonly string hostName;
        private string machineName;
        private string machineLocation;
        private string diskPath;
        private string networkAdapterName;
        private int memorySize;
        private string mac;

        public CreateMachineTask(string host)
        {
            hostName = host;
        }

        public static CreateMachineTask New(string host, string machineName)
        {
            CreateMachineTask task = new CreateMachineTask(host);
            return task.Name(machineName);
        }

        public static CreateMachineTask New(
            string host, 
            string machineName, 
            string machineLocation, 
            string diskPath,
            string networkAdapterName, 
            int memorySize)
        {
            CreateMachineTask task = new CreateMachineTask(host);
            return task.Name(machineName)
                .Location(machineLocation)
                .DiskPath(diskPath)
                .Network(networkAdapterName)
                .Memory(memorySize);
        }

        public static void Execute(
            ITaskContext environment, 
            string host, 
            string machineName,
            string machineLocation, 
            string diskPath, 
            string networkAdapterName, 
            int memorySize)
        {
            New(host, machineName, machineLocation, diskPath, networkAdapterName, memorySize)
                .Execute(environment);
        }

        /// <summary>
        ///   Set virtual machine name.
        /// </summary>
        /// <param name = "vmName">Name of the virtual machine.</param>
        /// <returns>This same instance of <see cref="CreateMachineTask"/></returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public CreateMachineTask Name(string vmName)
        {
            machineName = vmName;
            return this;
        }

        /// <summary>
        ///   Set virtual machine location (full path to virtual machine). It's hosts local path.
        /// </summary>
        /// <param name = "vmLocation">Hosts local path to virtual machine location.</param>
        /// <returns>This same instance of <see cref="CreateMachineTask"/></returns>
        [SuppressMessage ("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public CreateMachineTask Location(string vmLocation)
        {
            machineLocation = vmLocation;
            return this;
        }

        /// <summary>
        ///   Set full path to existing virtual disk file (host local path).
        ///   If null or empty new fixed size disk will be created in <see cref = "Location" />\<see cref = "Name" />.vhd location.
        /// </summary>
        /// <param name = "path">Full hosts local path to existing virtual disk.</param>
        /// <returns>This same instance of <see cref="CreateMachineTask"/></returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public CreateMachineTask DiskPath (string path)
        {
            diskPath = path;
            return this;
        }

        /// <summary>
        ///   Set hyper-v virtual network name.
        /// </summary>
        /// <param name = "networkName">Network name.</param>
        /// <returns>This same instance of <see cref="CreateMachineTask"/></returns>
        public CreateMachineTask Network (string networkName)
        {
            networkAdapterName = networkName;
            return this;
        }

        /// <summary>
        ///   Set virtual machine memory size.
        /// </summary>
        /// <param name = "size">Memory size.</param>
        /// <returns>This same instance of <see cref="CreateMachineTask"/></returns>
        public CreateMachineTask Memory (int size)
        {
            memorySize = size;
            return this;
        }

        public CreateMachineTask MacAddress(string networkMac)
        {
            mac = networkMac;
            return this;
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Create virtual machine. Host:{0},Machine:{1},Location:{2}",
                    hostName, 
                    machineName, 
                    machineLocation);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (string.IsNullOrEmpty(hostName))
                throw new TaskExecutionException("Host name can not be empty!");
            if (string.IsNullOrEmpty(machineName))
                throw new TaskExecutionException("Machine name can not be empty!");
            if (string.IsNullOrEmpty(machineLocation))
                throw new TaskExecutionException("Machine location can not be empty!");
            using (HyperVManager manager = new HyperVManager())
            {
                manager.Connect(hostName);

                if (string.IsNullOrEmpty(diskPath))
                {
                    diskPath = Path.Combine(machineLocation, machineName + ".vhd");
                    IVirtualTask disk = manager.CreateDynamicDisk(diskPath, 50);
                    disk.WaitForCompletion(new TimeSpan(0, 0, 0, 10));
                }

                IVirtualTask t = manager.CreateVirtualMachine(
                    machineName, machineLocation, diskPath, networkAdapterName, mac, memorySize);
                t.WaitForCompletion(new TimeSpan(0, 0, 1, 0));
            }
        }
    }
}