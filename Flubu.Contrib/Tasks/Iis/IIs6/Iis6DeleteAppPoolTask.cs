using System;
using System.DirectoryServices;
using System.Globalization;
using System.IO;

namespace Flubu.Tasks.Iis.Iis6
{
    public class Iis6DeleteAppPoolTask : TaskBase, IDeleteAppPoolTask
    {
        public string ApplicationPoolName { get; set; }
        public bool FailIfNotExist { get; set; }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Delete application pool '{0}'.",
                    ApplicationPoolName);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            const string AppPoolsRootName = @"IIS://localhost/W3SVC/AppPools";

            using (DirectoryEntry parent = new DirectoryEntry(AppPoolsRootName))
            {
                DirectoryEntry applicationPoolEntry = null;

                try
                {
                    try
                    {
                        applicationPoolEntry = parent.Children.Find(ApplicationPoolName, "IIsApplicationPool");
                        applicationPoolEntry.DeleteTree();
                        applicationPoolEntry.CommitChanges();
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // application pool already exists
                        if (FailIfNotExist)
                        {
                            throw new RunnerFailedException(
                                String.Format(
                                    CultureInfo.InvariantCulture,
                                    "Application '{0}' does not exist.",
                                    ApplicationPoolName));
                        }

                        context.WriteInfo(
                            "Application pool '{0}' does not exist, doing nothing.",
                            ApplicationPoolName);
                        return;
                    }
                }
                finally
                {
                    if (applicationPoolEntry != null)
                        applicationPoolEntry.Dispose();
                }

                parent.CommitChanges();
            }
        }
    }
}