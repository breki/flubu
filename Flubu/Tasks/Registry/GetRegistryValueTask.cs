using System;
using Microsoft.Win32;

namespace Flubu.Tasks.Registry
{
    public class GetRegistryValueTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return string.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Get registry value '{0}@{1}' to configuration setting '{2}'", 
                    registryKeyPath, 
                    registryValueName, 
                    configurationSettingName);
            }
        }

        public GetRegistryValueTask (
            RegistryKey rootKey,
            string registryKeyPath,
            string registryValueName,
            string configurationSettingName)
        {
            this.rootKey = rootKey;
            this.registryKeyPath = registryKeyPath;
            this.registryValueName = registryValueName;
            this.configurationSettingName = configurationSettingName;
        }

        public static void Execute (
            ITaskContext context,
            RegistryKey rootKey,
            string registryKeyPath,
            string registryValueName,
            string configurationSettingName)
        {
            GetRegistryValueTask task = new GetRegistryValueTask (
                rootKey, 
                registryKeyPath, 
                registryValueName,
                configurationSettingName);
            task.Execute (context);
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (RegistryKey key = rootKey.OpenSubKey (registryKeyPath, false))
            {
                if (key == null)
                    throw new TaskExecutionException (
                        string.Format (
                            System.Globalization.CultureInfo.InvariantCulture,
                            "Registry key '{0}' does not exist.", 
                            registryKeyPath));

                context.Properties.Set(
                    configurationSettingName, 
                    Convert.ToString (key.GetValue (registryValueName), System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        private readonly RegistryKey rootKey;
        private readonly string registryKeyPath;
        private readonly string registryValueName;
        private readonly string configurationSettingName;
    }
}
