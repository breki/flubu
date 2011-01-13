using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7ControlAppPoolTask : TaskBase, IControlAppPoolTask
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
                return string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{1} application pool '{0}'.",
                    applicationPoolName,
                    action);
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;

                foreach (ApplicationPool applicationPool in applicationPoolCollection)
                {
                    if (applicationPool.Name == ApplicationPoolName)
                    {
                        switch (action)
                        {
                            case ControlApplicationPoolAction.Start:
                                RunWithRetries(x => applicationPool.Start(), 3);
                                break;
                            case ControlApplicationPoolAction.Stop:
                                RunWithRetries(x => applicationPool.Stop(), 3, 0x80070426);
                                break;
                            case ControlApplicationPoolAction.Recycle:
                                RunWithRetries(x => applicationPool.Recycle(), 3);
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        serverManager.CommitChanges();

                        context.WriteInfo(
                            "Application pool '{0}' has been deleted.",
                            ApplicationPoolName);

                        return;
                    }
                }

                string message = String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Application pool '{0}' does not exist.",
                    applicationPoolName);

                if (failIfNotExist)
                    throw new TaskExecutionException(message);
                
                context.WriteInfo(message);
            }
        }

        private static void RunWithRetries(
            Action<int> action, 
            int retries, 
            params uint[] ignoredErrorCodes)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    action(0);
                    break;
                }
                catch (COMException ex)
                {
                    for (int j = 0; j < ignoredErrorCodes.Length; j++)
                    {
                        if (ignoredErrorCodes[j] == ex.ErrorCode)
                            return;
                    }

                    if (i == retries-1)
                        throw;
                    Thread.Sleep(1000);
                }
            }
        }

        private string applicationPoolName;
        private ControlApplicationPoolAction action;
        private bool failIfNotExist;
    }
}