using System;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7CreateAppPoolTask : TaskBase, ICreateAppPoolTask
    {
        public string ApplicationPoolName
        {
            get { return applicationPoolName; }
            set { applicationPoolName = value; }
        }

        public bool ClassicManagedPipelineMode { get; set; }

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

        protected override void DoExecute(ITaskContext context)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                ApplicationPoolCollection applicationPoolCollection = serverManager.ApplicationPools;

                ApplicationPool appPoolToWorkOn = null;
                bool updatedExisting = false;

                foreach (ApplicationPool applicationPool in applicationPoolCollection)
                {
                    if (applicationPool.Name == applicationPoolName)
                    {
                        if (mode == CreateApplicationPoolMode.DoNothingIfExists)
                        {
                            context.WriteInfo(
                                "Application pool '{0}' already exists, doing nothing.",
                                applicationPoolName);
                        }
                        else if (mode == CreateApplicationPoolMode.FailIfAlreadyExists)
                        {
                            throw new RunnerFailedException(
                                String.Format(
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Application '{0}' already exists.",
                                    applicationPoolName));
                        }

                        // otherwise we should update the existing application pool
                        appPoolToWorkOn = applicationPool;
                        updatedExisting = true;
                        break;
                    }
                }

                if (appPoolToWorkOn == null)
                    appPoolToWorkOn = serverManager.ApplicationPools.Add(applicationPoolName);

                appPoolToWorkOn.AutoStart = false;
                appPoolToWorkOn.Enable32BitAppOnWin64 = true;
                appPoolToWorkOn.ManagedPipelineMode =
                    ClassicManagedPipelineMode ? ManagedPipelineMode.Classic : ManagedPipelineMode.Integrated;
                //serverManager.ApplicationPools.Add(appPoolToWorkOn);
                serverManager.CommitChanges();

                context.WriteInfo(
                    "Application pool '{0}' {1}.",
                    applicationPoolName,
                    updatedExisting ? "updated" : "created");
            }
        }

        private string applicationPoolName;
        private CreateApplicationPoolMode mode;
    }
}