using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace Flubu.Tasks.Registry
{
    public class EditRegistryValueTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture, 
                    "Set registry value '{0}@{1}' = '{2}'", 
                    registryKeyPath, 
                    registryValueName, 
                    registryValueValue);
            }
        }

        public EditRegistryValueTask (
            RegistryKey rootKey, 
            string registryKeyPath, 
            string registryValueName, 
            object registryValueValue)
        {
            this.rootKey = rootKey;
            this.registryKeyPath = registryKeyPath;
            this.registryValueName = registryValueName;
            this.registryValueValue = registryValueValue;
        }

        public static void Execute (
            ITaskContext context, 
            RegistryKey rootKey, 
            string registryKeyPath, 
            string registryValueName, 
            object registryValueValue)
        {
            EditRegistryValueTask task = new EditRegistryValueTask (
                rootKey, 
                registryKeyPath, 
                registryValueName, 
                registryValueValue);
            task.Execute (context);
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (RegistryKey key = rootKey.OpenSubKey (registryKeyPath, true))
            {
                if (key == null)
                    throw new TaskExecutionException (
                        String.Format (
                            System.Globalization.CultureInfo.InvariantCulture, 
                            "Registry key '{0}' does not exist.", 
                            registryKeyPath));

                key.SetValue (registryValueName, registryValueValue);
            }
        }

        private readonly RegistryKey rootKey;
        private readonly string registryKeyPath;
        private readonly string registryValueName;
        private readonly object registryValueValue;
    }
}
