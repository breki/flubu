using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Text;

namespace Flubu.Tasks.EnterpriseServices
{
    public class UninstallAssemblyTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Uninstall COM+ assembly {0}", 
                    assemblyName);
            }
        }

        public UninstallAssemblyTask (string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        protected override void DoExecute (ITaskContext context)
        {
            RegistrationHelper regHelper = new RegistrationHelper ();
            regHelper.UninstallAssembly (assemblyName, null);
        }

        private string assemblyName;
    }
}
