using System;
using System.DirectoryServices;

namespace Flubu.Tasks.Iis
{
    public class DeleteVirtualDirectoryTask : TaskBase
    {
        public string ParentVirtualDirectoryName
        {
            get { return parentVirtualDirectoryName; }
            set { parentVirtualDirectoryName = value; }
        }

        public override string Description
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Delete IIS virtual directory '{0}'",
                    virtualDirectoryName);
            }
        }

        public DeleteVirtualDirectoryTask (string virtualDirectoryName, bool failIfNotExist)
        {
            this.virtualDirectoryName = virtualDirectoryName;
            this.failIfNotExist = failIfNotExist;
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (DirectoryEntry parent = new DirectoryEntry (parentVirtualDirectoryName))
            {
                object[] parameters = { "IIsWebVirtualDir", virtualDirectoryName };
                try
                {
                    parent.Invoke ("Delete", parameters);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    if (ex.InnerException is System.IO.DirectoryNotFoundException
                        && false == failIfNotExist)
                        return;

                    throw;
                }
            }
        }

        private readonly string virtualDirectoryName;
        private readonly bool failIfNotExist;
        private string parentVirtualDirectoryName = @"IIS://localhost/W3SVC/1/Root";
    }
}
