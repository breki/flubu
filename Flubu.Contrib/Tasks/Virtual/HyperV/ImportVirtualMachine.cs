using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management;

namespace Flubu.Tasks.Virtual.HyperV
{
    public class ImportVirtualMachine
    {
        private readonly string host;

        public ImportVirtualMachine(string virtualHost)
        {
            host = virtualHost;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "ImportDirectory")]
        private static ManagementBaseObject GetVirtualSystemImportSettingData(
            ManagementScope scope, string importDirectory, string machineName, string machineLocation)
        {
            string targetVhdResourcePath = machineLocation + "\\" + machineName + ".vhd";
                //Directories specified should exist
            ManagementObject virtualSystemService = Utility.GetServiceObject(scope,
                                                                             "Msvm_VirtualSystemManagementService");
            ManagementBaseObject importSettingData;
            ManagementBaseObject inParams = virtualSystemService.GetMethodParameters("GetVirtualSystemImportSettingData");
            inParams["ImportDirectory"] = importDirectory;

            ManagementBaseObject outParams = virtualSystemService.InvokeMethod("GetVirtualSystemImportSettingData",
                                                                               inParams, null);
            if (outParams == null) throw new ArgumentException("WMI call returned null!");
            uint ret = (UInt32) outParams["ReturnValue"];
            if (ret == ReturnCode.Started)
            {
                if (Utility.JobCompleted(outParams, scope))
                {
                    importSettingData = (ManagementBaseObject) outParams["ImportSettingData"];
                }
                else
                {
                    throw new NotSupportedException("Failed to get the Import Setting Data");
                }
            }
            else if (ret == ReturnCode.Completed)
            {
                importSettingData = (ManagementBaseObject) outParams["ImportSettingData"];
            }
            else
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                                              "Failed to get the Import Setting Data for the Import Directory :{0}",
                                                              ret));
            }

            inParams.Dispose();
            outParams.Dispose();
            virtualSystemService.Dispose();

            importSettingData["GenerateNewId"] = false;
            importSettingData["CreateCopy"] = true;
            importSettingData["Name"] = machineName;
            importSettingData["TargetResourcePaths"] = new[] {(targetVhdResourcePath)};

            return importSettingData;
        }

        public void ImportVirtualSystem(string importDirectory, string newMachineName, string machineLocation)
        {
            var scope = new ManagementScope(@"\\" + host + @"\root\virtualization", null);
            using (
                ManagementObject virtualSystemService = Utility.GetServiceObject(scope,
                                                                                 "Msvm_VirtualSystemManagementService"))
            {
                ManagementBaseObject importSettingData = GetVirtualSystemImportSettingData(scope, importDirectory,
                                                                                           newMachineName,
                                                                                           machineLocation);

                using (ManagementBaseObject inParams = virtualSystemService.GetMethodParameters("ImportVirtualSystemEx")
                    )
                {
                    inParams["ImportDirectory"] = importDirectory;
                    inParams["ImportSettingData"] = importSettingData.GetText(TextFormat.CimDtd20);

                    using (
                        ManagementBaseObject outParams = virtualSystemService.InvokeMethod("ImportVirtualSystemEx",
                                                                                           inParams, null))
                    {
                        if (outParams == null) throw new ArgumentException("WMI call returned null!");
                        uint ret = (UInt32) outParams["ReturnValue"];
                        if (ret == ReturnCode.Started)
                        {
                            if (!Utility.JobCompleted(outParams, scope))
                                throw new NotSupportedException("Failed to Import VM");
                        }
                        else if (ret != ReturnCode.Completed)
                        {
                            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                                                                          "Import virtual system failed with error:{0}",
                                                                          ret));
                        }
                    }
                }
            }
        }
    }
}