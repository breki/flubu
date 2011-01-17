using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Flubu.Tasks.Virtual.HyperV
{
    public interface IVirtualManager
    {
        /// <summary>
        ///   Gets current executing task.
        /// </summary>
        IVirtualTask CurrentTask { get; }

        /// <summary>
        ///   Gets full list of executed task in this session.
        /// </summary>
        ICollection<IVirtualTask> TaskList { get; }

        /// <summary>
        ///   Connect to remote instance of Virtual server from local server using DCOM
        /// </summary>
        void Connect(string server);

        /// <summary>
        ///   Connect to instance of Virtual server from local server using COM
        /// </summary>
        void Connect();

        /// <summary>
        ///   Creates new differencing disk.
        /// </summary>
        /// <param name = "newImage">Full path (with disk image name) of new virtual disk image.</param>
        /// <param name = "baseImage">Full path of base virtual disk image to use.</param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask CreateDifferencingDisk(string newImage, string baseImage);

        /// <summary>
        ///   Creates new fixed size disk.
        /// </summary>
        /// <param name = "newImage">Full path (with disk image name) of new virtual disk image.</param>
        /// <param name = "size">Size of new virtual disk in bytes</param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask CreateFixedDisk(string newImage, int size);

        /// <summary>
        ///   Creates new dynamic size disk.
        /// </summary>
        /// <param name = "newImage">Full path (with disk image name) of new virtual disk image.</param>
        /// <param name = "size">Size of new virtual disk in bytes</param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask CreateDynamicDisk(string newImage, int size);

        /// <summary>
        ///   Creates new virtual machine.
        /// </summary>
        /// <param name = "machineName">Name of the virtual machine to crate.</param>
        /// <param name = "machineFolder">Full path where virtual machine will be created.</param>
        /// <param name = "diskPath">Full path and name of disk image to use.</param>
        /// <param name = "networkAdapterName">Name of the network adapter to use.</param>
        /// <param name = "memorySize">Memory size to use for new virtual machine.</param>
        /// <param name = "macAddress"></param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask CreateVirtualMachine(string machineName, string machineFolder, string diskPath,
                                   string networkAdapterName, string macAddress, int memorySize);

        /// <summary>
        ///   Creates new virtual machine with configuration from existing virtual machine.
        /// </summary>
        /// <param name = "machineName">Name of the virtual machine to crate.</param>
        /// <param name = "machineFolder">Full path where virtual machine will be created.</param>
        /// <param name = "baseMachineName">Name of the base virtual machine name.</param>
        /// <param name = "diskPath">Full path and name of disk image to use.</param>
        /// <param name = "networkAdapterName">Name of the network adapter to use.</param>
        /// <param name = "macAddress"></param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask CloneVirtualMachine(string machineName, string machineFolder, string baseMachineName, string diskPath,
                                  string networkAdapterName, string macAddress);

        /// <summary>
        ///   Deletes existing virtual machine from virtual server.
        ///   <remarks>
        ///     Disk image files are not deleted.
        ///   </remarks>
        /// </summary>
        /// <param name = "machineName">Name of the virtual machine to delete.</param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask DeleteVirtualMachine(string machineName);

        /// <summary>
        ///   Starts virtual machine
        /// </summary>
        /// <param name = "machineName"></param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask StartVirtualMachine(string machineName);

        /// <summary>
        ///   Turnoff specified virtual machine.
        /// </summary>
        /// <param name = "machineName"></param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask TurnoffVirtualMachine(string machineName);

        /// <summary>
        ///   Shutdown specified virtual machine.
        /// </summary>
        /// <param name = "machineName"></param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask ShutdownVirtualMachine(string machineName);

        /// <summary>
        ///   Save virtual machine state.
        /// </summary>
        /// <param name = "machineName"></param>
        /// <returns>New <see cref = "IVirtualTask" /> for current operation.</returns>
        IVirtualTask SaveVirtualMachine(string machineName);

        /// <summary>
        ///   Gets all registered virtual machines on server.
        /// </summary>
        /// <returns>Collection of <see cref = "VirtualMachine" /></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Collection<VirtualMachine> GetVirtualMachines();
    }
}