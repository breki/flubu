﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7CreateWebApplicationTask : Iis7TaskBase, ICreateWebApplicationTask
    {
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

        /// <summary>
        /// Gets or sets the Name of the website that the web application is added too. By default it is "Default Web Site"
        /// </summary>
        public string WebsiteName
        {
            get { return websiteName; }
            set { websiteName = value; }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<MimeType> MimeTypes { get; set; }

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
            if (string.IsNullOrEmpty(ApplicationName))
            {
                throw new TaskExecutionException("ApplicationName missing!");
            }

            using (ServerManager serverManager = new ServerManager())
            {
                if (!WebsiteExists(serverManager, WebsiteName))
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, "Web site '{0}' does not exists.", WebsiteName));
                }

                Site site = serverManager.Sites[WebsiteName];

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
                    Site defaultSite = manager.Sites[WebsiteName];
                    Application ourApplication = defaultSite.Applications.Add(vdirPath, this.LocalPath);
                    ourApplication.ApplicationPoolName = applicationPoolName;
                    var config = ourApplication.GetWebConfiguration();
                    AddMimeTypes(config, MimeTypes);
                    manager.CommitChanges();
                }
            }
        }

        private CreateWebApplicationMode mode = CreateWebApplicationMode.FailIfAlreadyExists;
        private string applicationName;
        private string parentVirtualDirectoryName = @"IIS://localhost/W3SVC/1/Root";
        private string localPath;
        private bool allowAnonymous = true;
        private bool allowAuthNtlm = true;
        private bool accessScript = true;
        private string anonymousUserName;
        private string anonymousUserPass;
        private string appFriendlyName;
        private bool aspEnableParentPaths;
        private string defaultDoc;
        private bool enableDefaultDoc = true;
        private string applicationPoolName = "DefaultAppPool";
        private string websiteName = "Default Web Site";
    }
}