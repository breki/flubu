using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;

namespace Flubu.Tasks.Iis.Iis6
{
    // IIS metabase properties
    // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/iissdk/html/bb9c0d25-d003-4ddd-8adb-8662de0a24ee.asp
    // http://west-wind.com/weblog/posts/399.aspx

    public class Iis6CreateWebApplicationTask : TaskBase, ICreateWebApplicationTask
    {
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
            set { localPath = value; }
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

        public string WebSiteName
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public IList<MimeTYPE> MimeTypes
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException();  }
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
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Create IIS Web application '{0}' on local path '{1}'",
                    applicationName, 
                    localPath);
            }
        }

        protected override void DoExecute (ITaskContext context)
        {
            string parentName = parentVirtualDirectoryName;

            using (DirectoryEntry parent = new DirectoryEntry (parentName))
            {
                DirectoryEntry virtualDirEntry = null;

                try
                {
                    // first check if the virtual directory already exists
                    try
                    {
                        virtualDirEntry = parent.Children.Find (applicationName, "IIsWebVirtualDir");

                        // virtual directory already exists
                        if (mode == CreateWebApplicationMode.DoNothingIfExists)
                        {
                            context.WriteInfo(
                                "Web application '{0}' already exists, doing nothing.", 
                                applicationName);
                            return;
                        }
                        else if (mode == CreateWebApplicationMode.FailIfAlreadyExists)
                        {
                            throw new TaskExecutionException (
                                String.Format (
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    "Web application '{0}' already exists.", 
                                    applicationName));
                        }
                        // otherwise we should update the existing virtual directory
                    }
                    catch (DirectoryNotFoundException)
                    {
                        // virtual directory does not exist, go on and add it
                        virtualDirEntry = parent.Children.Add (applicationName, "IIsWebVirtualDir");
                    }

                    virtualDirEntry.Properties["Path"][0] = localPath;
                    virtualDirEntry.CommitChanges ();
                    
                    if (!string.IsNullOrEmpty(applicationPoolName))
                        virtualDirEntry.Invoke ("AppCreate3", new object[] { 2, applicationPoolName, false });

                    virtualDirEntry.Properties["AppFriendlyName"][0] = appFriendlyName ?? applicationName;

                    int authFlags = 0;
                    if (allowAnonymous)
                        authFlags |= 1;
                    if (allowAuthNtlm)
                        authFlags |= 4;

                    virtualDirEntry.Properties["AuthFlags"][0] = authFlags;
                    if (anonymousUserName != null)
                        virtualDirEntry.Properties["AnonymousUserName"][0] = anonymousUserName;
                    if (anonymousUserPass != null)
                        virtualDirEntry.Properties["AnonymousUserPass"][0] = anonymousUserPass;
                    if (defaultDoc != null)
                        virtualDirEntry.Properties["DefaultDoc"][0] = defaultDoc;
                    virtualDirEntry.Properties["EnableDefaultDoc"][0] = enableDefaultDoc;

                    virtualDirEntry.Properties["AccessScript"][0] = true;
                    virtualDirEntry.Properties["AccessExecute"][0] = true;

                    virtualDirEntry.Properties["AspEnableParentPaths"][0] = aspEnableParentPaths;

                    virtualDirEntry.CommitChanges ();
                }
                finally
                {
                    if (virtualDirEntry != null)
                        virtualDirEntry.Dispose ();
                }

                parent.CommitChanges ();
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
    }
}
