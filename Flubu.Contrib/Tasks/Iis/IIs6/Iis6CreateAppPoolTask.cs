using System;
using System.DirectoryServices;
using System.IO;

namespace Flubu.Tasks.Iis.Iis6
{
    public class Iis6CreateAppPoolTask : TaskBase, ICreateAppPoolTask
    {
        public string ApplicationPoolName
        {
            get { return applicationPoolName; }
            set { applicationPoolName = value; }
        }

        public bool ClassicManagedPipelineMode
        {
            get { return classicManagedPipelineMode; }
            set { classicManagedPipelineMode = value; }
        }

        public CreateApplicationPoolMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Create application pool '{0}'.", 
                    applicationPoolName);
            }
        }

        protected override void DoExecute (ITaskContext context)
        {
            string version = GetLocalIisVersionTask.GetIisVersion(context, false);
            int major = GetLocalIisVersionTask.GetMajorVersion(version);
            if (major < 6 || major == 0)
            {
                context.WriteInfo("IIS does not support application pools.");
                return;
            } 
            
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

                        // application pool already exists
                        if (mode == CreateApplicationPoolMode.DoNothingIfExists)
                        {
                            context.WriteInfo(
                                "Application pool '{0}' already exists, doing nothing.", 
                                applicationPoolName);
                            return;
                        }
                        else if (mode == CreateApplicationPoolMode.FailIfAlreadyExists)
                        {
                            throw new RunnerFailedException (
                                String.Format (
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Application '{0}' already exists.", 
                                    applicationPoolName));
                        }

                        // otherwise we should update the existing application pool
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // application pool does not exist, go on and add it
                        applicationPoolEntry = parent.Children.Add (applicationPoolName, "IIsApplicationPool");
                    }

                    if (major > 6)
                    {
                        applicationPoolEntry.InvokeSet(
                            "ManagedPipelineMode",
                            new object[] { classicManagedPipelineMode ? 1 : 0 });
                    }

                    applicationPoolEntry.CommitChanges ();
                }
                finally
                {
                    if (applicationPoolEntry != null)
                        applicationPoolEntry.Dispose ();
                }
            
                parent.CommitChanges ();
            }
        }

        private string applicationPoolName;
        private bool classicManagedPipelineMode;
        private CreateApplicationPoolMode mode;
    }
}
