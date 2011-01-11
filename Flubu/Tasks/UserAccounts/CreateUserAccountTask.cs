using System;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace Flubu.Tasks.UserAccounts
{
    // http://www.codeproject.com/dotnet/addnewuser.asp
    // http://www.codeproject.com/useritems/everythingInAD.asp
    public class CreateUserAccountTask : TaskBase
    {
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Create user account '{0}'", 
                    userName);
            }
        }

        public CreateUserAccountTask (
            CreateUserAccountMode mode,
            string userName,
            string password,
            string userDescription)
        {
            this.mode = mode;
            this.userName = userName;
            this.password = password;
            this.userDescription = userDescription;
        }

        public static void Execute (
            ITaskContext environment, 
            CreateUserAccountMode mode,
            string userName, 
            string password, 
            string userDescription)
        {
            CreateUserAccountTask task = new CreateUserAccountTask (mode, userName, password, userDescription);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (DirectoryEntry rootDirectoryEntry = new DirectoryEntry ("WinNT://" + Environment.MachineName + ",computer"))
            {
                DirectoryEntry userEntry = null;

                try
                {
                    // first check if the user already exists
                    try
                    {
                        userEntry = rootDirectoryEntry.Children.Find (userName, "user");

                        // user already exists
                        if (mode == CreateUserAccountMode.DoNothingIfExists)
                        {
                            context.WriteInfo(
                                "User '{0}' already exists, doing nothing.", 
                                userName);
                            return;
                        }
                        else if (mode == CreateUserAccountMode.FailIfAlreadyExists)
                        {
                            throw new TaskExecutionException (
                                String.Format (
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "User '{0}' already exists.", 
                                    userName));
                        }

                        // otherwise we should update the existing user
                    }
                    catch (COMException ex)
                    {
                        if (ex.ErrorCode == -2147022675)
                        {
                            // user does not exist, go on and add it
                            userEntry = rootDirectoryEntry.Children.Add (userName, "user");
                        }
                        else
                            throw;
                    }

                    const int ADS_UF_DONT_EXPIRE_PASSWD = 0x00010000;
                    const int ADS_UF_PASSWD_CANT_CHANGE = 0x00000040;

                    userEntry.Invoke ("SetPassword", new object[] { password });
                    userEntry.Invoke ("Put", new object[] { "Description", userDescription });
                    userEntry.Invoke ("Put", new object[] { "userFlags", ADS_UF_DONT_EXPIRE_PASSWD | ADS_UF_PASSWD_CANT_CHANGE });

                    userEntry.CommitChanges ();
                }
                finally
                {
                    if (userEntry != null)
                        userEntry.Dispose ();
                }
            }
        }

        private CreateUserAccountMode mode;
        private string userName;
        private string password;
        private string userDescription;
    }
}
