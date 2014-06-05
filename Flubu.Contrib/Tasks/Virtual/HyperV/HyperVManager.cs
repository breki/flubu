using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management;

namespace Flubu.Tasks.Virtual.HyperV
{
    public class HyperVManager : IVirtualManager, IDisposable
    {
        public IVirtualTask CurrentTask
        {
            get { return currentTask; }
        }

        public ICollection<IVirtualTask> TaskList
        {
            get { return taskList; }
        }

        public Collection<VirtualMachine> GetVirtualMachines()
        {
            var list = new Collection<VirtualMachine>();
            var query = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem");

            using (var searcher = new ManagementObjectSearcher(managementScope, query))
            {
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    var name = (string)m["ElementName"];
                    if (name.ToUpperInvariant().Equals(serverName.ToUpperInvariant()))
                        continue;

                    list.Add(new VirtualMachine
                                 {
                                     Name = name,
                                     Id = (string)m["Name"],
                                     Status = (VirtualMachineState)(UInt16)m["EnabledState"]
                                 });
                }

                return list;
            }
        }

        /// <summary>
        /// Connect to remote instance of Virtual server from local server using DCOM
        /// </summary>
        /// <param name="server">
        /// The server.
        /// </param>
        public void Connect(string server)
        {
            if (string.IsNullOrEmpty(server)) throw new ArgumentNullException("server");
            serverName = server;
            managementScope = new ManagementScope(@"\\" + serverName + @"\root\virtualization");
            managementScope.Connect();
        }

        /// <summary>
        ///   Connect to instance of Virtual server from local server using COM
        /// </summary>
        public void Connect()
        {
            Connect(".");
        }

        /// <summary>
        ///   Creates new differencing disk.
        /// </summary>
        /// <param name = "newImage">Full path (with disk image name) of new virtual disk image.</param>
        /// <param name = "baseImage">Full path of base virtual disk image to use.</param>
        /// <returns>The task</returns>
        public IVirtualTask CreateDifferencingDisk(string newImage, string baseImage)
        {
            if (string.IsNullOrEmpty(newImage)) throw new ArgumentNullException("newImage");
            if (string.IsNullOrEmpty(baseImage)) throw new ArgumentNullException("baseImage");

            using (ManagementObject imageService = Utility.GetServiceObject(managementScope, "Msvm_ImageManagementService"))
            using (ManagementBaseObject inParams = imageService.GetMethodParameters("CreateDifferencingVirtualHardDisk"))
            {
                inParams["ParentPath"] = baseImage;
                inParams["Path"] = newImage;
                using (ManagementBaseObject outParams = imageService.InvokeMethod("CreateDifferencingVirtualHardDisk", inParams, null))
                {
                    return NewTask(outParams, managementScope);
                }
            }
        }

        /// <summary>
        ///   Creates new fixed size disk.
        /// </summary>
        /// <param name = "newImage">Full path (with disk image name) of new virtual disk image.</param>
        /// <param name = "size">Size of new virtual disk in Gb</param>
        /// <returns>The task</returns>
        public IVirtualTask CreateFixedDisk(string newImage, int size)
        {
            if (string.IsNullOrEmpty(newImage)) throw new ArgumentNullException("newImage");
            
            const UInt64 Size1G = 0x40000000;
            using (ManagementObject imageService = Utility.GetServiceObject(managementScope, "Msvm_ImageManagementService"))
            using (ManagementBaseObject inParams = imageService.GetMethodParameters("CreateFixedVirtualHardDisk"))
            {
                inParams["Path"] = newImage;
                inParams["MaxInternalSize"] = (UInt64)size*Size1G;
                using (ManagementBaseObject outParams = imageService.InvokeMethod("CreateFixedVirtualHardDisk", inParams, null))
                {
                    return NewTask(outParams, managementScope);
                }
            }
    }

        /// <summary>
        ///   Creates new dynamic size disk.
        /// </summary>
        /// <param name = "newImage">Full path (with disk image name) of new virtual disk image.</param>
        /// <param name = "size">Size of new virtual disk in Gb</param>
        /// <returns>The task.</returns>
        public IVirtualTask CreateDynamicDisk(string newImage, int size)
        {
            if (string.IsNullOrEmpty(newImage)) throw new ArgumentNullException("newImage");
            
            const UInt64 Size1G = 0x40000000;

            using (ManagementObject imageService = Utility.GetServiceObject(managementScope, "Msvm_ImageManagementService"))
            using (ManagementBaseObject inParams = imageService.GetMethodParameters("CreateDynamicVirtualHardDisk"))
            {
                inParams["Path"] = newImage;
                inParams["MaxInternalSize"] = (UInt64)size*Size1G;
                using (ManagementBaseObject outParams = imageService.InvokeMethod("CreateDynamicVirtualHardDisk", inParams, null))
                {
                    return NewTask(outParams, managementScope);
                }
            }
        }

        /// <summary>
        ///   Creates new virtual machine.
        /// </summary>
        /// <param name = "machineName">Name of the virtual machine to crate.</param>
        /// <param name = "machineFolder">Full path where virtual machine will be created.</param>
        /// <param name = "diskPath">Full path and name of disk image to use.</param>
        /// <param name = "networkAdapterName">Name of the network adapter to use.</param>
        /// <param name = "macAddress">MAC address</param>
        /// <param name = "memorySize">Memory size to use for new virtual machine.</param>
        /// <returns>The task</returns>
        public IVirtualTask CreateVirtualMachine(
            string machineName, string machineFolder, string diskPath, string networkAdapterName, string macAddress, int memorySize)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            if (string.IsNullOrEmpty(machineFolder)) throw new ArgumentNullException("machineFolder");
            if (string.IsNullOrEmpty(diskPath)) throw new ArgumentNullException("diskPath");

            using (ManagementObject newVm = FindVirtualMachine(machineName))
            {
                if (newVm != null)
                    throw new ArgumentOutOfRangeException("machineName", machineName, "Virtual machine already exists.");
            }

            using (ManagementObject virtualSystemService = Utility.GetServiceObject(managementScope, "Msvm_VirtualSystemManagementService"))
            using (ManagementBaseObject inParams = virtualSystemService.GetMethodParameters("DefineVirtualSystem"))
            {
                inParams["ResourcesettingData"] = null;
                inParams["Sourcesetting"] = null;
                inParams["SystemsettingData"] = GetVirtualSystemGlobalSettingDataInstance(managementScope, machineName);

                using (ManagementBaseObject outParams = virtualSystemService.InvokeMethod("DefineVirtualSystem", inParams, null))
                {
                    if (outParams == null)
                        throw new ArgumentException("Could not execute creation of new virtual machine!");

                    if ((UInt32)outParams["ReturnValue"] == ReturnCode.Started)
                        NewTask(outParams, managementScope);

                    NewTask((UInt32)outParams["ReturnValue"]);
                }
            }

            ManagementObject vm = Utility.GetTargetComputer(machineName, managementScope);
            AddVirtualHarddrive(managementScope, vm, diskPath);
            AddVirtualNetwork(machineName, networkAdapterName, macAddress);
            return currentTask;
        }

        /// <summary>
        ///   Creates new virtual machine with configuration from existing virtual machine.
        /// </summary>
        /// <param name = "machineName">Name of the virtual machine to crate.</param>
        /// <param name = "machineFolder">Full path where virtual machine will be created.</param>
        /// <param name = "baseMachineName">Name of the base virtual machine name.</param>
        /// <param name = "diskPath">Full path and name of base disk image to use.</param>
        /// <param name = "networkAdapterName">Name of the network adapter to use.</param>
        /// <param name = "macAddress">MAC address</param>
        /// <returns>The task</returns>
        public IVirtualTask CloneVirtualMachine(
            string machineName, 
            string machineFolder, 
            string baseMachineName,
            string diskPath, 
            string networkAdapterName, 
            string macAddress)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            if (string.IsNullOrEmpty(baseMachineName)) throw new ArgumentNullException("baseMachineName");

            using (ManagementObject newVm = FindVirtualMachine(machineName))
            {
                if (newVm != null)
                    throw new ArgumentOutOfRangeException("machineName", machineName, "Virtual machine already exists.");
            }

            using (ManagementObject baseVm = FindVirtualMachine(baseMachineName))
            {
                if (baseVm == null)
                    throw new ArgumentOutOfRangeException("baseMachineName", baseMachineName, "Base virtual machine does not exist.");
            }

            using (ManagementObject virtualSystemService = Utility.GetServiceObject(managementScope, "Msvm_VirtualSystemManagementService"))
            using (ManagementBaseObject inParams = virtualSystemService.GetMethodParameters("DefineVirtualSystem"))
            {
                inParams["ResourcesettingData"] = null;
                inParams["Sourcesetting"] = null;
                inParams["SystemsettingData"] = GetVirtualSystemGlobalSettingDataInstance(managementScope, machineName);

                using (ManagementBaseObject outParams = virtualSystemService.InvokeMethod("DefineVirtualSystem", inParams, null))
                {
                    if (outParams == null)
                    {
                        throw new ArgumentException("Could not execute creation of new virtual machine!");
                    }

                    if ((UInt32)outParams["ReturnValue"] == ReturnCode.Started)
                    {
                        NewTask(outParams, managementScope);
                    }

                    NewTask((UInt32)outParams["ReturnValue"]);
                }
            }

            ManagementObject vm = Utility.GetTargetComputer(machineName, managementScope);

            string newDisk = Path.Combine(machineFolder, machineName + ".vhd");
            IVirtualTask task = CreateDifferencingDisk(newDisk, diskPath);
            task.WaitForCompletion(new TimeSpan(0, 0, 0, 10));
            AddVirtualHarddrive(managementScope, vm, newDisk);
            ConnectSwitchPort connect = new ConnectSwitchPort(serverName);
            connect.Connect(
                networkAdapterName, 
                networkAdapterName + "_ExternalPort", 
                machineName, 
                "synthetic",
                baseMachineName);

            return currentTask;
        }

        /// <summary>
        ///   Deletes existing virtual machine from virtual server.
        ///   <remarks>
        ///     Disk image files are not deleted.
        ///   </remarks>
        /// </summary>
        /// <param name = "machineName">Name of the virtual machine to delete.</param>
        /// <returns>The task</returns>
        public IVirtualTask DeleteVirtualMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");

            using (ManagementObject service = Utility.GetServiceObject(managementScope, "Msvm_VirtualSystemManagementService"))
            using (ManagementBaseObject inP = service.GetMethodParameters("DestroyVirtualSystem"))
            using (ManagementObject vm = Utility.GetTargetComputer(machineName, managementScope))
            {
                if (vm == null || vm.Path == null)
                    throw new TaskExecutionException("Virtual machine {0} does not exist.", machineName);

                inP["ComputerSystem"] = vm.Path.Path;
                using (ManagementBaseObject outP = service.InvokeMethod("DestroyVirtualSystem", inP, null))
                {
                    if (outP == null)
                        throw new ArgumentException("Could not delete virtual machine!");

                    if ((UInt32)outP["ReturnValue"] == ReturnCode.Started)
                        return NewTask(outP, managementScope);

                    return NewTask((UInt32)outP["ReturnValue"]);
                }
            }
        }

        /// <summary>
        ///   Starts virtual machine
        /// </summary>
        /// <param name = "machineName">Machine name</param>
        /// <returns>The task</returns>
        public IVirtualTask StartVirtualMachine (string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            return RequestStateChange(machineName, StateChange.TurnOn);
        }

        public IVirtualTask TurnoffVirtualMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            return RequestStateChange(machineName, StateChange.Turnoff);
        }

        public IVirtualTask ShutdownVirtualMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            ManagementObject vm = FindVirtualMachine(machineName);
            if (vm == null)
            {
                throw new ArgumentOutOfRangeException("machineName", "Virtual machine not found.");
            }

            var guid = (string)vm["Name"];

            var query = new ObjectQuery("SELECT * FROM Msvm_ShutdownComponent WHERE SystemName='" + guid + "'");

            using (var searcher = new ManagementObjectSearcher(managementScope, query))
            {
                ManagementObject component = null;
                using (ManagementObjectCollection queryCollection = searcher.Get())
                {
                    foreach (ManagementObject m in queryCollection)
                    {
                        component = m;
                        break;
                    }

                    if (component == null)
                    {
                        throw new ArgumentOutOfRangeException("machineName", "Shutdown component not found.");
                    }

                    using (ManagementBaseObject inParams = component.GetMethodParameters("InitiateShutdown"))
                    {
                        inParams["Force"] = true;
                        inParams["Reason"] = "test";

                        using (
                            ManagementBaseObject outParams = component.InvokeMethod("InitiateShutdown", inParams, null))
                        {
                            if (outParams == null)
                            {
                                throw new ArgumentException("Error invoking WMI RequestStateChange method.");
                            }

                            return NewTask((UInt32)outParams["ReturnValue"]);
                        }
                    }
                }
            }
        }

        public IVirtualTask SaveVirtualMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            return RequestStateChange(machineName, StateChange.Suspend);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static string GetVirtualSystemGlobalSettingDataInstance(ManagementScope scope, string machineName)
        {
            var settingPath = new ManagementPath("Msvm_VirtualSystemGlobalsettingData");

            using (var globalSettingClass = new ManagementClass(scope, settingPath, null))
            {
                using (ManagementObject globalSettingData = globalSettingClass.CreateInstance())
                {
                    if (globalSettingData == null)
                    {
                        throw new ArgumentException("Could not get global machine settings.");
                    }

                    globalSettingData["ElementName"] = machineName;

                    return globalSettingData.GetText(TextFormat.CimDtd20);
                }
            }
        }

        private IVirtualTask NewTask(UInt32 returnValue)
        {
            currentTask = new HyperVTask(returnValue);
            taskList.Add(currentTask);
            return currentTask;
        }

        private IVirtualTask NewTask(ManagementBaseObject outParameters, ManagementScope scope)
        {
            currentTask = new HyperVTask(outParameters, scope);
            taskList.Add(currentTask);
            return currentTask;
        }

        private ManagementObject FindVirtualMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            var query = new ObjectQuery("SELECT * FROM Msvm_ComputerSystem WHERE ElementName='" + machineName + "'");

            using (var searcher = new ManagementObjectSearcher(managementScope, query))
            {
                ManagementObject vm = null;
                ManagementObjectCollection queryCollection = searcher.Get();
                foreach (ManagementObject m in queryCollection)
                {
                    vm = m;
                    break;
                }

                return vm;
            }
        }

        /// <summary>
        ///   Starts or turnoff virtual machine
        /// </summary>
        /// <param name = "vmName">Virtual machine name</param>
        /// <param name = "state">State change</param>
        /// <returns>Task for starting or stopping virtual machine.</returns>
        private IVirtualTask RequestStateChange(string vmName, StateChange state)
        {
            if (string.IsNullOrEmpty(vmName)) throw new ArgumentNullException("vmName");
            ManagementObject vm = FindVirtualMachine(vmName);
            if (vm == null)
            {
                throw new ArgumentOutOfRangeException("vmName", "Virtual machine not found.");
            }

            ManagementBaseObject inParams = vm.GetMethodParameters("RequestStateChange");

            inParams["RequestedState"] = (int)state;

            ManagementBaseObject outParams = vm.InvokeMethod("RequestStateChange", inParams, null);
            if (outParams == null)
            {
                throw new ArgumentException("Error invoking WMI RequestStateChange method.");
            }

            return NewTask(outParams, managementScope);
        }

        private ManagementObject AddVirtualSystemResource(
            ManagementScope scope, ManagementObject virtualMachine, ManagementObject resourceToAdd)
        {
            var resourcesToAddList = new ManagementObject[1];

            resourcesToAddList[0] = resourceToAdd;

            string[] resources = AddVirtualSystemResources(scope, virtualMachine, resourcesToAddList);
            if (resources != null)
            {
                return new ManagementObject(scope, new ManagementPath(resources[0]), null);
            }

            return null;
        }

        //********************************************************************************************************************

        private string[] AddVirtualSystemResources(
            ManagementScope scope, 
            ManagementObject virtualMachine,
            ManagementObject[] resourcesToAdd)
        {
            using (ManagementObject service = Utility.GetServiceObject(scope, "Msvm_VirtualSystemManagementService"))
            using (ManagementBaseObject inParams = service.GetMethodParameters("AddVirtualSystemResources"))
            {
                int idx = resourcesToAdd.GetLength(0);

                var resourcesToAddString = new string[idx];

                idx = 0;

                foreach (ManagementObject resource in resourcesToAdd)
                {
                    resourcesToAddString[idx++] = resource.GetText(TextFormat.CimDtd20);
                }

                inParams["ResourcesettingData"] = resourcesToAddString;

                inParams["TargetSystem"] = virtualMachine.Path.Path;

                using (ManagementBaseObject outParams = service.InvokeMethod("AddVirtualSystemResources", inParams, null))
                {
                    if (outParams == null)
                    {
                        throw new ArgumentException("Could not add new resource to virtual system!");
                    }

                    if ((UInt32)outParams["ReturnValue"] == ReturnCode.Started)
                    {
                        NewTask(outParams, managementScope);
                    }
                    else
                    {
                        NewTask((UInt32)outParams["ReturnValue"]);
                    }

                    return (string[])outParams["NewResources"];
                }
            }
        }

        //********************************************************************************************************************

        private void AddVirtualHarddrive(ManagementScope scope, ManagementObject virtualMachine, string vhdToMount)
        {
            // Locate the IDE controller on the vm

            using (ManagementObject controller = Utility.GetResourceAllocationSettingData(
                virtualMachine, ResourceType.IDEController, ResourceSubType.IDEController, null))
            {
                if (controller == null)
                {
                    // IDE controller does not exits on the vm, create it

                    ManagementObject toAdd = Utility.GetResourceDataDefault(
                        scope, ResourceType.IDEController, ResourceSubType.IDEController, null);

                    AddVirtualSystemResource(scope, virtualMachine, toAdd);
                }
            }

            using (ManagementObject controller = Utility.GetResourceAllocationSettingData(
                virtualMachine, ResourceType.IDEController, ResourceSubType.IDEController, null))
            {
                if (controller == null)
                {
                    throw new ArgumentException("No IDE controller!");
                }
                // Create the Synthetic disk drive on the IDE controller

                const int DiskLocation = 0;

                using (ManagementObject driveDefault = Utility.GetResourceDataDefault(
                    scope, ResourceType.Disk, ResourceSubType.DiskSynthetic, null))
                {
                    driveDefault["Parent"] = controller.Path;
                    driveDefault["Address"] = DiskLocation;
                    driveDefault["Limit"] = 1; // Not sure what this does???
                    ManagementObject newDiskDrive = AddVirtualSystemResource(scope, virtualMachine, driveDefault);

                    // Now create a new virtual hard disk, associate it with the new synthetic disk drive and attach the virtual hard drive to the virtual machine
                    ManagementObject vhdDefault = Utility.GetResourceDataDefault(
                        scope, ResourceType.StorageExtent, ResourceSubType.VHD, null);

                    vhdDefault["Parent"] = newDiskDrive;
                    var connection = new string[1];
                    connection[0] = vhdToMount;
                    vhdDefault["Connection"] = connection;
                    AddVirtualSystemResource(scope, virtualMachine, vhdDefault);
                }
            }
        }

        private void AddVirtualNetwork(string vmName, string nicName, string address)
        {
            if (string.IsNullOrEmpty(vmName)) throw new ArgumentNullException("vmName");

            using (ManagementObject service = Utility.GetServiceObject (managementScope, "Msvm_VirtualSystemManagementService"))
            using (ManagementObject vm = Utility.GetTargetComputer (vmName, managementScope))
            using (ManagementBaseObject inP = service.GetMethodParameters ("AddVirtualSystemResources"))
            using (ManagementObject nicDefault = Utility.GetResourceDataDefault (
                managementScope, ResourceType.EthernetAdapter, ResourceSubType.EthernetSynthetic, null))
            {
                if (address != null)
                {
                    nicDefault["StaticMacAddress"] = true;
                    nicDefault["Address"] = address;
                }
                else
                {
                    nicDefault["StaticMacAddress"] = false;
                }

                nicDefault["ElementName"] = nicName;
                var identifiers = new String[1];
                identifiers[0] = string.Format (CultureInfo.InvariantCulture, "{{{0}}}", Guid.NewGuid ());
                nicDefault["VirtualSystemIdentifiers"] = identifiers;

                var rasDs = new string[1];
                rasDs[0] = nicDefault.GetText (TextFormat.CimDtd20);

                inP["ResourcesettingData"] = rasDs;
                inP["TargetSystem"] = vm.Path.Path;

                ManagementBaseObject outP = service.InvokeMethod ("AddVirtualSystemResources", inP, null);
                if (outP == null)
                {
                    throw new ArgumentException ("Could not add virtual network!");
                }

                if ((UInt32)outP["ReturnValue"] == ReturnCode.Started)
                {
                    NewTask (outP, managementScope);
                }

                NewTask ((UInt32)outP["ReturnValue"]);
            }

            //ConnectSwitchPort connect = new ConnectSwitchPort(serverName);
            //connect.Connect(nicName, nicName + "_ExternalPort", vmName, "synthetic", null);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (currentTask != null)
            {
                currentTask.Dispose();
            }
        }

        private readonly List<IVirtualTask> taskList = new List<IVirtualTask>();
        private HyperVTask currentTask;
        private ManagementScope managementScope;
        private string serverName;
    }
}
