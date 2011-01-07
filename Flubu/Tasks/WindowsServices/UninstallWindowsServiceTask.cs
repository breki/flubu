using System;
using System.Collections;

namespace Flubu.Tasks.WindowsServices
{
    public class UninstallWindowsServiceTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Uninstall Windows service '{0}'", 
                    executablePath);
            }
        }

        public UninstallWindowsServiceTask (string executablePath)
        {
            this.executablePath = executablePath;
        }

        protected override void DoExecute (ITaskContext context)
        {
            IDictionary savedState = new Hashtable ();
            string[] commandLine = new string[0];

            using (System.Configuration.Install.AssemblyInstaller assemblyInstaller
                = new System.Configuration.Install.AssemblyInstaller (executablePath, commandLine))
            {
                assemblyInstaller.UseNewContext = true;

                assemblyInstaller.Uninstall (savedState);
            }
        }

        private string executablePath;
    }
}
