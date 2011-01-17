using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management;

namespace Flubu.Tasks.Virtual.HyperV
{
    public class ConnectSwitchPort
    {
        private readonly ManagementScope scope;
        private readonly ManagementObject switchService;

        public ConnectSwitchPort(string serverName)
        {
            scope = new ManagementScope(@"\\" + serverName + @"\root\virtualization");
            switchService = Utility.GetServiceObject(scope, "Msvm_VirtualSwitchManagementService");
        }

        private ManagementObject GetVirtualSwitch(string switchName)
        {
            if (switchName == null) throw new ArgumentNullException("switchName");
            string query = string.Format(CultureInfo.InvariantCulture,
                                         "select * from Msvm_VirtualSwitch where ElementName = '{0}'", switchName);

            using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query)))
            {
                ManagementObjectCollection virtualSwitchs = searcher.Get();

                ManagementObject virtualSwitch = null;

                foreach (ManagementObject instance in virtualSwitchs)
                {
                    virtualSwitch = instance;
                    break;
                }

                return virtualSwitch;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static ManagementObject GetVirtualSwitchPort(ManagementObject virtualSwitch, string switchPortName)
        {
            if (virtualSwitch == null) throw new ArgumentNullException("virtualSwitch");
            if (switchPortName == null) throw new ArgumentNullException("switchPortName");
            ManagementObjectCollection switchPorts = virtualSwitch.GetRelated
                (
                    "Msvm_SwitchPort",
                    "Msvm_HostedAccessPoint",
                    null,
                    null,
                    "Dependent",
                    "Antecedent",
                    false,
                    null
                );

            ManagementObject switchPort = null;

            foreach (ManagementObject instance in switchPorts)
            {
                if (instance["ElementName"].ToString().ToLowerInvariant() == switchPortName.ToLowerInvariant())
                {
                    switchPort = instance;
                    break;
                }
            }

            return switchPort;
        }

        private ManagementObject GetVmEthernetPort(string vmName, string vmEthernetPortClass)
        {
            if (vmName == null) throw new ArgumentNullException("vmName");
            if (vmEthernetPortClass == null) throw new ArgumentNullException("vmEthernetPortClass");
            ManagementObject vmEthernetPort = null;
            ManagementObject computerSystem = Utility.GetTargetComputer(vmName, scope);

            ManagementObjectCollection vmEthernetPorts = computerSystem.GetRelated
                (
                    vmEthernetPortClass,
                    "Msvm_SystemDevice",
                    null,
                    null,
                    "PartComponent",
                    "GroupComponent",
                    false,
                    null
                );

            foreach (ManagementObject instance in vmEthernetPorts)
            {
                vmEthernetPort = instance;
                break;
            }

            return vmEthernetPort;
        }

        private static ManagementObject GetVmLanEndPoint(ManagementObject vmEthernetPort)
        {
            if (vmEthernetPort == null) throw new ArgumentNullException("vmEthernetPort");
            ManagementObjectCollection vmEndPoints = vmEthernetPort.GetRelated
                (
                    "Msvm_VMLanEndPoint",
                    "Msvm_DeviceSAPImplementation",
                    null,
                    null,
                    "Dependent",
                    "Antecedent",
                    false,
                    null
                );

            ManagementObject vmEndPoint = null;

            foreach (ManagementObject instance in vmEndPoints)
            {
                vmEndPoint = instance;
                break;
            }

            return vmEndPoint;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Console.WriteLine(System.String,System.Object)")]
        private void ConnectSwitch(ManagementObject switchPort, ManagementObject lanEndPoint)
        {
            if (switchPort == null) throw new ArgumentNullException("switchPort");
            if (lanEndPoint == null) throw new ArgumentNullException("lanEndPoint");
            using (ManagementBaseObject inParams = switchService.GetMethodParameters("ConnectSwitchPort"))
            {
                inParams["SwitchPort"] = switchPort.Path.Path;
                inParams["LANEndpoint"] = lanEndPoint.Path.Path;
                using (ManagementBaseObject outParams = switchService.InvokeMethod("ConnectSwitchPort", inParams, null))
                {
                    Console.WriteLine(
                        (UInt32) outParams["ReturnValue"] == ReturnCode.Completed
                            ? "{0} was connected successfully"
                            : "Failed to connect {0} switch port.", switchPort.Path.Path);
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Vm"),
         SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vm"),
         SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase"),
         SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "vm")]
        public void Connect(string switchName, string switchPortName, string vmName, string vmNicType, string baseVmName)
        {
            if (switchName == null) throw new ArgumentNullException("switchName");
            if (switchPortName == null) throw new ArgumentNullException("switchPortName");
            if (vmName == null) throw new ArgumentNullException("vmName");
            if (vmNicType == null) throw new ArgumentNullException("vmNicType");
            string vmNicClassName = null;

            if (vmNicType.ToLowerInvariant() == "emulated")
            {
                vmNicClassName = "Msvm_EmulatedEthernetPort";
            }
            else if (vmNicType.ToLowerInvariant() == "synthetic")
            {
                vmNicClassName = "Msvm_SyntheticEthernetPort";
            }

            using (ManagementObject virtualSwitch = GetVirtualSwitch(switchName))
            {
                using (ManagementObject virtualSwitchPort = GetVirtualSwitchPort(virtualSwitch, switchPortName))
                {
                    using (ManagementObject vmNic = GetVmEthernetPort(baseVmName, vmNicClassName))
                    {
                        using (ManagementObject vmLanEndPoint = GetVmLanEndPoint(vmNic))
                        {
                            ConnectSwitch(virtualSwitchPort, vmLanEndPoint);
                        }
                    }
                }
            }
        }
    }
}