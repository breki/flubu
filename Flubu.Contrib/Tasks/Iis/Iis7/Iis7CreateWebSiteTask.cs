using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7CreateWebsiteTask : Iis7TaskBase, ICreateWebsiteTask
    {
        /// <summary>
        /// Name of the website
        /// </summary>
        private string webSiteName;

        /// <summary>
        /// The binding protocol. Http or https.
        /// </summary>
        private string bindingProtocol;

        /// <summary>
        /// Port of the web application
        /// </summary>
        private int port;

        /// <summary>
        /// Physical path to application.
        /// </summary>
        private string physicalPath;

        private string applicationPoolName = "DefaultAppPool";

        private IList<MimeType> mimeTypes;

        private CreateWebApplicationMode siteMode = CreateWebApplicationMode.DoNothingIfExists;
       
        public Iis7CreateWebsiteTask()
        {
            mimeTypes = new List<MimeType>();
        }

        public override string Description
        {
            get { return "Creates new Web site in iis."; }
        }

        public CreateWebsiteBindingProtocol WebsiteName(string siteName)
        {
            webSiteName = siteName;
            return new CreateWebsiteBindingProtocol(this);
        }

        public Iis7CreateWebsiteTask WebsiteMode(CreateWebApplicationMode value)
        {
            siteMode = value;
            return this;
        }

        public Iis7CreateWebsiteTask ApplicationPoolName(string applicationPool)
        {
            applicationPoolName = applicationPool;
            return this;
        }

        public Iis7CreateWebsiteTask AddMimeType(MimeType mimeType)
        {
            mimeTypes.Add(mimeType);
            return this;
        }

        /// <summary>
        /// Creates or updated the web site.
        /// </summary>
        /// <param name="context">The task context</param>
        protected override void DoExecute(ITaskContext context)
        {
            Validate();
            using (ServerManager serverManager = new ServerManager())
            {
                var webSiteExists = WebsiteExists(serverManager, webSiteName);

                if (siteMode == CreateWebApplicationMode.DoNothingIfExists && webSiteExists)
                {
                    return;
                }

                if (siteMode == CreateWebApplicationMode.FailIfAlreadyExists && webSiteExists)
                {
                    throw new TaskExecutionException(
                        string.Format(CultureInfo.InvariantCulture, "web site {0} already exists!", webSiteName));
                }

                if (siteMode == CreateWebApplicationMode.UpdateIfExists && webSiteExists)
                {
                   serverManager.Sites[webSiteName].Delete();
                }

                var bindingInformation = string.Format(CultureInfo.InvariantCulture, "*:{0}:", port);
                var site = serverManager.Sites.Add(webSiteName, bindingProtocol, bindingInformation, physicalPath);
                site.ApplicationDefaults.ApplicationPoolName = applicationPoolName;
                serverManager.CommitChanges();

                Microsoft.Web.Administration.Configuration config = site.GetWebConfiguration();
                AddMimeTypes(config, mimeTypes);
                serverManager.CommitChanges();
            }
        }

        /// <summary>
        /// Validates Ii7CreateWebSiteTask properties.
        /// </summary>
        private void Validate()
        {
            if (string.IsNullOrEmpty(webSiteName))
            {
                throw new TaskExecutionException("webSiteName missing!");
            }

            if (string.IsNullOrEmpty(bindingProtocol))
            {
                throw new TaskExecutionException("bindingProtocol missing!");
            }

            if (port == 0)
            {
                throw new TaskExecutionException("Port missing!");
            }

            if (string.IsNullOrEmpty(physicalPath))
            {
                throw new TaskExecutionException("physicalPath missing!");
            }

            if (!bindingProtocol.Equals("http", StringComparison.OrdinalIgnoreCase)
                && !bindingProtocol.Equals ("https", StringComparison.OrdinalIgnoreCase))
            {
                throw new TaskExecutionException("Wrong binding protocol. Supported values http and https");
            }
        }

        public class CreateWebsiteBindingProtocol
        {
            private readonly Iis7CreateWebsiteTask task;
            public CreateWebsiteBindingProtocol(Iis7CreateWebsiteTask task)
            {
                this.task = task;
            }

            /// <summary>
            /// Sets the binding protocol. Http or https.
            /// </summary>
            /// <param name="value">The binding protocol. Supported values http and https.</param>
            /// <returns>new instance of <see cref="CreateWebsiteProtocol"/></returns>
            public CreateWebsiteProtocol BindingProtocol(string value)
            {
                task.bindingProtocol = value;
                return new CreateWebsiteProtocol(task);
            }
        }
        
        public class CreateWebsiteProtocol
        {
            private readonly Iis7CreateWebsiteTask task;
          
            public CreateWebsiteProtocol(Iis7CreateWebsiteTask task)
            {
                this.task = task;
            }

            /// <summary>
            /// Sets the port of the web site.
            /// </summary>
            /// <param name="value">The port</param>
            /// <returns>New instance of <see cref="CreateWebsitePhysicalPath"/></returns>
            public CreateWebsitePhysicalPath Port(int value)
            {
                task.port = value;
                return new CreateWebsitePhysicalPath(task);
            }
        }

        public class CreateWebsitePhysicalPath
        {
            private readonly Iis7CreateWebsiteTask task;

            public CreateWebsitePhysicalPath(Iis7CreateWebsiteTask task)
            {
                this.task = task;
            }

            /// <summary>
            /// Sets the physical path to the web site.
            /// </summary>
            /// <param name="value">The physical path.</param>
            /// <returns>The iI7CreateWebSiteTask.</returns>
            public Iis7CreateWebsiteTask PhysicalPath(string value)
            {
                task.physicalPath = value;
                return task;
            }
        }
    }
}
