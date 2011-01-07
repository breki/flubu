using System;
using System.Globalization;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7DeleteAppPoolTask : TaskBase, IDeleteAppPoolTask
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
            ServerManager serverManager = new ServerManager();
            ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;

            foreach (ApplicationPool applicationPool in applicationPoolCollection)
            {
                if (applicationPool.Name == ApplicationPoolName)
                {
                    applicationPoolCollection.Remove(applicationPool);
                    serverManager.CommitChanges();

                    context.WriteInfo(
                        "Application pool '{0}' has been deleted.",
                        ApplicationPoolName);     

                    return;
                }
            }

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
        }
    }
}