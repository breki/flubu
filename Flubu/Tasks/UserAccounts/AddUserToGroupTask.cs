using System;
using System.DirectoryServices;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Flubu.Tasks.UserAccounts
{
    public class AddUserToGroupTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Add user account '{0}' to group '{1}'", 
                    userName, 
                    group);
            }
        }

        public static void Execute (ITaskContext environment, string userName, string group)
        {
            AddUserToGroupTask task = new AddUserToGroupTask ();
            task.userName = userName;
            task.group = group;
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (DirectoryEntry computerDirectoryEntry = new DirectoryEntry ("WinNT://" + Environment.MachineName + ",computer"))
            {
                using (DirectoryEntry userEntry = computerDirectoryEntry.Children.Find (userName, "user"))
                {
                    using (DirectoryEntry grp = computerDirectoryEntry.Children.Find (group, "group"))
                    {
                        try
                        {
                            grp.Invoke ("Add", new object[] { userEntry.Path.ToString () });
                        }
                        catch (TargetInvocationException ex)
                        {
                            if (ex.InnerException is COMException)
                            {
                                if ((ex.InnerException as COMException).ErrorCode == -2147023518)
                                {
                                    // user is already member of the group, nothing to do
                                    context.WriteInfo(
                                        "User '{0}' is already member of the group '{1}', nothing to do.",
                                        userName, 
                                        group);
                                    return;
                                }
                            }

                            throw;
                        }
                    }
                }
            }
        }

        private string userName;
        private string group;
    }
}
