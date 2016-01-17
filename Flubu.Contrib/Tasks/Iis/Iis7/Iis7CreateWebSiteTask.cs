using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7CreateWebSiteTask : Iis7TaskBase, ICreateWebSiteTask
    {
        private readonly string webSiteName;

        private readonly string bindingProtocol;

        private readonly int port;

        private readonly string physicalPath;

        private string applicationPoolName = "DefaultAppPool";

        private IList<MimeTYPE> mimeTypes;

        private CreateWebApplicationMode siteMode = CreateWebApplicationMode.DoNothingIfExists;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iis7CreateWebSiteTask"/> class.
        /// </summary>
        /// <param name="webSiteName">Name of the website</param>
        /// <param name="bindingProtocol">The binding protocol. Http or https.</param>
        /// <param name="port">Port of the web application</param>
        /// <param name="physicalPath">Physical path to application.</param>
        public Iis7CreateWebSiteTask(string webSiteName, string bindingProtocol, int port, string physicalPath)
        {
            this.webSiteName = webSiteName;
            this.bindingProtocol = bindingProtocol;
            this.port = port;
            this.physicalPath = physicalPath;
            mimeTypes = new List<MimeTYPE>();
        }

        public override string Description
        {
            get { return "Creates new Web site in iis."; }
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
    }
}
