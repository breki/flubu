using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management;
using System.Threading;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Hyper-V utility class.
    /// </summary>
    internal static class Utility
    {
        [SuppressMessage ("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static ManagementObject GetServiceObject (ManagementScope scope, string serviceName)
        {
            if (scope == null) throw new ArgumentNullException ("scope");
            scope.Connect ();
            var wmiPath = new ManagementPath (serviceName);
            var serviceClass = new ManagementClass (scope, wmiPath, null);
            ManagementObjectCollection services = serviceClass.GetInstances ();

            ManagementObject serviceObject = null;

            foreach (ManagementObject service in services)
            {
                serviceObject = service;
            }

            return serviceObject;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static ManagementObject GetSystemDevice (
            string deviceClassName,
            string deviceObjectElementName,
            string virtualMachineName,
            ManagementScope scope)
        {
            if (deviceObjectElementName == null) throw new ArgumentNullException ("deviceObjectElementName");
            ManagementObject systemDevice = null;
            ManagementObject computerSystem = GetTargetComputer (virtualMachineName, scope);

            ManagementObjectCollection systemDevices = computerSystem.GetRelated (
                    deviceClassName,
                    "Msvm_SystemDevice",
                    null,
                    null,
                    "PartComponent",
                    "GroupComponent",
                    false,
                    null);

            foreach (ManagementObject device in systemDevices)
            {
                if (device["ElementName"].ToString ().ToUpperInvariant () == deviceObjectElementName.ToUpperInvariant ())
                {
                    systemDevice = device;
                    break;
                }
            }

            return systemDevice;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"),
         SuppressMessage ("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"),
         SuppressMessage ("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
             MessageId = "System.Console.WriteLine(System.String,System.Object)")]
        public static bool JobCompleted (ManagementBaseObject outParameters, ManagementScope scope)
        {
            if (outParameters == null) throw new ArgumentNullException ("outParameters");

            //Retrieve msvc_StorageJob path. This is a full wmi path
            var jobPath = (string)outParameters["Job"];
            var job = new ManagementObject (scope, new ManagementPath (jobPath), null);
            //Try to get storage job information
            job.Get ();
            while ((UInt16)job["JobState"] == JobState.Starting
                   || (UInt16)job["JobState"] == JobState.Running)
            {
                Thread.Sleep (1000);
                job.Get ();
            }

            //Figure out if job failed
            var jobState = (UInt16)job["JobState"];
            if (jobState != JobState.Completed)
            {
                throw new VirtualServerException (job["ErrorCode"].ToString (), job["ErrorDescription"].ToString ());
            }

            return true;
        }

        [SuppressMessage ("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static ManagementObject GetTargetComputer (string virtualMachineElementName, ManagementScope scope)
        {
            string query = string.Format (
                CultureInfo.InvariantCulture,
                "select * from Msvm_ComputerSystem Where ElementName = '{0}'",
                virtualMachineElementName);

            var searcher = new ManagementObjectSearcher (scope, new ObjectQuery (query));

            ManagementObjectCollection computers = searcher.Get ();

            ManagementObject computer = null;

            foreach (ManagementObject instance in computers)
            {
                computer = instance;
                break;
            }

            return computer;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static ManagementObject GetVirtualSystemSettingData (ManagementObject virtualMachine)
        {
            if (virtualMachine == null) throw new ArgumentNullException ("virtualMachine");
            ManagementObject vmSetting = null;
            ManagementObjectCollection vmSettings = virtualMachine.GetRelated (
                    "Msvm_VirtualSystemSettingData",
                    "Msvm_SettingsDefineState",
                    null,
                    null,
                    "SettingData",
                    "ManagedElement",
                    false,
                    null);

            if (vmSettings.Count != 1)
            {
                throw new ArgumentException (
                    string.Format (CultureInfo.InvariantCulture, "{0} instance of Msvm_VirtualSystemSettingData was found", vmSettings.Count));
            }

            foreach (ManagementObject instance in vmSettings)
            {
                vmSetting = instance;
                break;
            }

            return vmSetting;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static ManagementObject GetHostSystemDevice (
            string deviceClassName, string deviceObjectElementName, ManagementScope scope)
        {
            string hostName = Environment.MachineName;
            ManagementObject systemDevice = GetSystemDevice (deviceClassName, deviceObjectElementName, hostName, scope);
            return systemDevice;
        }

        [SuppressMessage ("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static ManagementObject GetResourceDataDefault (
            ManagementScope scope,
            UInt16 resourceType,
            string resourceSubtype,
            string otherResourceType)
        {
            ManagementObject rasd = null;

            //=
            //    String.Format(
            //        "select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType ='{1}' and OtherResourceType = '{2}'",
            //        resourceType, resourceSubtype, otherResourceType);

            string query = resourceType == ResourceType.Other
                ? string.Format (
                CultureInfo.InvariantCulture,
                "select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType = null and OtherResourceType = {1}",
                resourceType, 
                otherResourceType)
                : string.Format (
                CultureInfo.InvariantCulture,
                "select * from Msvm_ResourcePool where ResourceType = '{0}' and ResourceSubType ='{1}' and OtherResourceType = null",
                resourceType, 
                resourceSubtype);

            ManagementObjectCollection poolResources;
            using (var searcher = new ManagementObjectSearcher (scope, new ObjectQuery (query)))
            {
                poolResources = searcher.Get ();
                //Get pool resource allocation ability
                if (poolResources.Count == 1)
                {
                    foreach (ManagementObject poolResource in poolResources)
                    {
                        ManagementObjectCollection allocationCapabilities =
                            poolResource.GetRelated ("Msvm_AllocationCapabilities");
                        foreach (ManagementObject allocationCapability in allocationCapabilities)
                        {
                            ManagementObjectCollection settingDatas =
                                allocationCapability.GetRelationships ("Msvm_SettingsDefineCapabilities");
                            foreach (ManagementObject settingData in settingDatas)
                            {
                                if (Convert.ToInt16 (settingData["ValueRole"], CultureInfo.InvariantCulture) ==
                                    (UInt16)ValueRole.Default)
                                {
                                    rasd = new ManagementObject (settingData["PartComponent"].ToString ());
                                    break;
                                }
                            }
                        }
                    }
                }

                return rasd;
            }
        }

        internal static ManagementObject GetResourceAllocationSettingData (
            ManagementObject virtualMachine,
            UInt16 resourceType,
            string resourceSubtype,
            string otherResourceType)
        {
            if (virtualMachine == null) throw new ArgumentNullException ("virtualMachine");
            //virtualMachine->vmsettings->RASD for IDE controller
            ManagementObject rasd = null;
            ManagementObjectCollection settingDatas = virtualMachine.GetRelated ("Msvm_VirtualSystemsettingData");
            foreach (ManagementObject settingData in settingDatas)
            {
                //retrieve the rasd
                ManagementObjectCollection rasDs = settingData.GetRelated ("Msvm_ResourceAllocationsettingData");
                foreach (ManagementObject rasdInstance in rasDs)
                {
                    if (Convert.ToUInt16 (rasdInstance["ResourceType"], CultureInfo.InvariantCulture) == resourceType)
                    {
                        //found the matching type
                        if (resourceType == ResourceType.Other)
                        {
                            if (rasdInstance["OtherResourceType"].ToString () == otherResourceType)
                            {
                                rasd = rasdInstance;
                                break;
                            }
                        }
                        else
                        {
                            if (rasdInstance["ResourceSubType"].ToString () == resourceSubtype)
                            {
                                rasd = rasdInstance;
                                break;
                            }
                        }
                    }
                }
            }

            return rasd;
        }

        #region Nested type: ValueRole

        private enum ValueRole
        {
            Default = 0,
        }

        #endregion Nested type: ValueRole
    }
}