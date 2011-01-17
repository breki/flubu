using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Virtual.HyperV;

namespace Flubu.Tasks.Virtual
{
    public class CloneMachineTask : TaskBase
    {
        private readonly string hostName;
        private string machineName;
        private string baseMachineName;
        private string machineLocation;
        private string diskPath;
        private string networkAdapterName;
        private string mac;

        public CloneMachineTask(string host)
        {
            hostName = host;
        }

        public static CloneMachineTask New(string host, string machineName)
        {
            CloneMachineTask task = new CloneMachineTask(host);
            return task.Name(machineName);
        }

        public static CloneMachineTask New(string host, string machineName, string baseMachineName,
                                           string machineLocation, string diskPath, string networkAdapterName)
        {
            CloneMachineTask task = new CloneMachineTask(host);
            return task.Name(machineName)
                .BaseName(baseMachineName)
                .Location(machineLocation)
                .DiskPath(diskPath)
                .Network(networkAdapterName);
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public CloneMachineTask Name(string vmName)
        {
            machineName = vmName;
            return this;
        }

        public CloneMachineTask BaseName(string name)
        {
            baseMachineName = name;
            return this;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public CloneMachineTask Location(string vmLocation)
        {
            machineLocation = vmLocation;
            return this;
        }

        public CloneMachineTask DiskPath(string path)
        {
            diskPath = path;
            return this;
        }

        public CloneMachineTask Network(string networkName)
        {
            networkAdapterName = networkName;
            return this;
        }

        public CloneMachineTask MacAddress(string networkMac)
        {
            mac = networkMac;
            return this;
        }

        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "Clone virtual machine. Host:{0},Machine:{1},Location:{2},Base:{3}",
                                     hostName, machineName, machineLocation, baseMachineName);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (string.IsNullOrEmpty(machineName))
                throw new TaskExecutionException("Machine name must be specified!");

            if (string.IsNullOrEmpty(machineLocation))
                throw new TaskExecutionException("Machine location must be specified!");
            if (string.IsNullOrEmpty(baseMachineName))
                throw new TaskExecutionException("Base machine name must be specified!");
            using (HyperVManager manager = new HyperVManager())
            {
                manager.Connect(hostName);

                IVirtualTask t = manager.CloneVirtualMachine(machineName, machineLocation, baseMachineName, diskPath,
                                                      networkAdapterName, mac);
                t.WaitForCompletion(new TimeSpan(0, 0, 1, 0));
            }
        }
    }
}