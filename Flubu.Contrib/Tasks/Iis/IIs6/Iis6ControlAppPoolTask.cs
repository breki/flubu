using System;
using System.DirectoryServices;
using System.IO;

namespace Flubu.Tasks.Iis.Iis6
{
    public class Iis6ControlAppPoolTask : TaskBase, IControlAppPoolTask
    {
        public string ApplicationPoolName
        {
            get { return applicationPoolName; }
            set { applicationPoolName = value; }
        }

        public ControlApplicationPoolAction Action
        {
            get { return action; }
            set { action = value; }
        }

        public bool FailIfNotExist
        {
            get { return failIfNotExist; }
            set { failIfNotExist = value; }
        }

        public override string Description
        {
            get
            {
                return string.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{1} application pool '{0}'.", 
                    applicationPoolName, 
                    action);
            }
        }

        protected override void DoExecute (ITaskContext context)
        {
            const string AppPoolsRootName = @"IIS://localhost/W3SVC/AppPools";

            using (DirectoryEntry parent = new DirectoryEntry (AppPoolsRootName))
            {
                DirectoryEntry applicationPoolEntry = null;

                try
                {
                    // first check if the user already exists
                    try
                    {
                        applicationPoolEntry = parent.Children.Find (applicationPoolName, "IIsApplicationPool");

                        applicationPoolEntry.Invoke (action.ToString ());
                    }
                    catch (DirectoryNotFoundException)
                    {
                        string message = String.Format (
                            System.Globalization.CultureInfo.InvariantCulture,
                            "Application pool '{0}' does not exist.", 
                            applicationPoolName);

                        if (failIfNotExist)
                            throw new TaskExecutionException (message);
                        else
                            context.WriteInfo(message);
                    }
                }
                finally
                {
                    if (applicationPoolEntry != null)
                        applicationPoolEntry.Dispose ();
                }
            }
        }

        private string applicationPoolName;
        private ControlApplicationPoolAction action;
        private bool failIfNotExist;
    }
}

