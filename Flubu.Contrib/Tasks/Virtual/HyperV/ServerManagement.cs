using System;
using System.Management;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Helper class for managing server machine.
    /// </summary>
    public static class ServerManagement
    {
        /// <summary>
        /// Shutdown windows machine.
        /// </summary>
        /// <param name = "machineName">Machine name</param>
        public static void Shutdown(string machineName)
        {
            if (string.IsNullOrEmpty(machineName)) throw new ArgumentNullException("machineName");
            ManagementScope managementScope = machineName.ToUpperInvariant() ==
                                              Environment.MachineName.ToUpperInvariant()
                                                  ? new ManagementScope(@"\ROOT\CIMV2")
                                                  : new ManagementScope(@"\\" + machineName + @"\ROOT\CIMV2");

            managementScope.Connect();
            var query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
            using (var searcher = new ManagementObjectSearcher(managementScope, query))
            {
                foreach (ManagementObject operatingSystem in searcher.Get())
                {
                    operatingSystem.InvokeMethod("Shutdown", null, null);
                }
            }
        }
    }
}