using System;
using System.Globalization;
using System.IO;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7CreateWebApplicationTask : TaskBase, ICreateWebApplicationTask
    {
        private bool accessScript = true;
        private bool allowAnonymous = true;
        private bool allowAuthNtlm = true;
        private string anonymousUserName;
        private string anonymousUserPass;
        private string appFriendlyName;
        private string applicationName;
        private string applicationPoolName = "DefaultAppPool";
        private bool aspEnableParentPaths;
        private string defaultDoc;
        private bool enableDefaultDoc = true;
        private string localPath;
        private CreateWebApplicationMode mode = CreateWebApplicationMode.FailIfAlreadyExists;
        private string parentVirtualDirectoryName = @"IIS://localhost/W3SVC/1/Root";

        #region ICreateWebApplicationTask Members

        public CreateWebApplicationMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public string LocalPath
        {
            get { return localPath; }
            set { localPath = Path.GetFullPath(value); }
        }

        public bool AllowAnonymous
        {
            get { return allowAnonymous; }
            set { allowAnonymous = value; }
        }

        public bool AllowAuthNtlm
        {
            get { return allowAuthNtlm; }
            set { allowAuthNtlm = value; }
        }

        public string AnonymousUserName
        {
            get { return anonymousUserName; }
            set { anonymousUserName = value; }
        }

        public string AnonymousUserPass
        {
            get { return anonymousUserPass; }
            set { anonymousUserPass = value; }
        }

        public string AppFriendlyName
        {
            get { return appFriendlyName; }
            set { appFriendlyName = value; }
        }

        public bool AspEnableParentPaths
        {
            get { return aspEnableParentPaths; }
            set { aspEnableParentPaths = value; }
        }

        public bool AccessScript
        {
            get { return accessScript; }
            set { accessScript = value; }
        }

        public bool AccessExecute { get; set; }

        public string DefaultDoc
        {
            get { return defaultDoc; }
            set { defaultDoc = value; }
        }

        public bool EnableDefaultDoc
        {
            get { return enableDefaultDoc; }
            set { enableDefaultDoc = value; }
        }

        public string ParentVirtualDirectoryName
        {
            get { return parentVirtualDirectoryName; }
            set { parentVirtualDirectoryName = value; }
        }

        public string ApplicationPoolName
        {
            get { return applicationPoolName; }
            set { applicationPoolName = value; }
        }

        public override string Description
        {
            get
            {
                return String.Format(
                    CultureInfo.InvariantCulture,
                    "Create IIS Web application '{0}' on local path '{1}'",
                    applicationName,
                    localPath);
            }
        }

        #endregion

        protected override void DoExecute(ITaskContext context)
        {
            using (ServerManager serverManager = new ServerManager())
            {
                Site site = serverManager.Sites["Default Web Site"];

                string vdirPath = "/" + ApplicationName;
                foreach (Application application in site.Applications)
                {
                    if (application.Path == vdirPath)
                    {
                        if (mode == CreateWebApplicationMode.DoNothingIfExists)
                        {
                            context.WriteInfo(
                                "Web application '{0}' already exists, doing nothing.",
                                applicationName);
                            return;
                        }

                        if (mode == CreateWebApplicationMode.FailIfAlreadyExists)
                        {
                            throw new TaskExecutionException(
                                String.Format(
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Web application '{0}' already exists.",
                                    applicationName));
                        }

                        // otherwise we should update the existing virtual directory
                        //TODO update existing application
                        //ourApplication = application;
                        return;
                    }

                }

                using (ServerManager manager = new ServerManager())
                {
                    Site defaultSite = manager.Sites["Default Web Site"];
                    Application ourApplication = defaultSite.Applications.Add(vdirPath, this.LocalPath);
                    ourApplication.ApplicationPoolName = applicationPoolName;
                    manager.CommitChanges();
                }
            }
        }
    }
}