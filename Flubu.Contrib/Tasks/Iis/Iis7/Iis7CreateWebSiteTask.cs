using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7CreateWebSiteTask : Iis7TaskBase, ICreateWebSiteTask
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

        private IList<MimeTYPE> mimeTypes;

        private CreateWebApplicationMode siteMode = CreateWebApplicationMode.DoNothingIfExists;
       
        public Iis7CreateWebSiteTask()
        {
            mimeTypes = new List<MimeTYPE>();
        }

        public override string Description
        {
            get { return "Creates new Web site in iis."; }
        }

        public CreateWebSiteBindingProtocol WebSiteName(string siteName)
        {
            webSiteName = siteName;
            return new CreateWebSiteBindingProtocol(this);
        }

        public Iis7CreateWebSiteTask WebSiteMode(CreateWebApplicationMode webSiteMode)
        {
            siteMode = webSiteMode;
            return this;
        }

        public Iis7CreateWebSiteTask ApplicationPoolName(string applicationPool)
        {
            applicationPoolName = applicationPool;
            return this;
        }

        public Iis7CreateWebSiteTask AddMimeType(MimeTYPE mimeType)
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
                    throw new TaskExecutionException(string.Format("web site {0} already exists!", webSiteName));
                }

                if (siteMode == CreateWebApplicationMode.UpdateIfExists && webSiteExists)
                {
                   serverManager.Sites[webSiteName].Delete();
                }

                var bindingInformation = string.Format("*:{0}", port);
                var site = serverManager.Sites.Add(webSiteName, bindingProtocol, bindingInformation, physicalPath);
                site.ApplicationDefaults.ApplicationPoolName = applicationPoolName;
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

            if (!bindingProtocol.Equals("http", StringComparison.InvariantCultureIgnoreCase)
                && !bindingProtocol.Equals("https", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new TaskExecutionException("Wrong binding protocol. Supported values http and https");
            }
        }

        public class CreateWebSiteBindingProtocol
        {
            private readonly Iis7CreateWebSiteTask task;
            public CreateWebSiteBindingProtocol(Iis7CreateWebSiteTask task)
            {
                this.task = task;
            }

            /// <summary>
            /// Sets the binding protocol. Http or https.
            /// </summary>
            /// <param name="bindingProtocol">The binding protocol. Supported values http and https.</param>
            /// <returns>new instance of <see cref="CreateWebSiteProtocol"/></returns>
            public CreateWebSiteProtocol BindingProtocol(string bindingProtocol)
            {
                task.bindingProtocol = bindingProtocol;
                return new CreateWebSiteProtocol(task);
            }
        }
        
        public class CreateWebSiteProtocol
        {
            private readonly Iis7CreateWebSiteTask task;
          
            public CreateWebSiteProtocol(Iis7CreateWebSiteTask task)
            {
                this.task = task;
            }

            /// <summary>
            /// Sets the port of the web site.
            /// </summary>
            /// <param name="port">The port</param>
            /// <returns>New instance of <see cref="CreateWebSitePhysicalPath"/></returns>
            public CreateWebSitePhysicalPath Port(int port)
            {
                task.port = port;
                return new CreateWebSitePhysicalPath(task);
            }
        }

        public class CreateWebSitePhysicalPath
        {
            private readonly Iis7CreateWebSiteTask task;

            public CreateWebSitePhysicalPath(Iis7CreateWebSiteTask task)
            {
                this.task = task;
            }

            /// <summary>
            /// Sets the physical path to the web site.
            /// </summary>
            /// <param name="physicalPath">The physical path.</param>
            /// <returns>The iI7CreateWebSiteTask.</returns>
            public Iis7CreateWebSiteTask PhysicalPath(string physicalPath)
            {
                task.physicalPath = physicalPath;
                return task;
            }
        }
    }
}
