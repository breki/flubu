using System.Globalization;
using System.Linq;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7AddWebsiteBindingTask : TaskBase, IAddWebsiteBindingTask
    {
        public override string Description
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Add '{0}' binding to web site '{1}'", protocol,
                                     siteName);
            }
        }

        private string siteName;
        private string protocol;

        public Iis7AddWebsiteBindingTask SiteName(string name)
        {
            siteName = name;
            return this;
        }

        public Iis7AddWebsiteBindingTask AddBinding(string protocol)
        {
            this.protocol = protocol;
            return this;
        }
        
        protected override void DoExecute(ITaskContext context)
        {
            if(string.IsNullOrEmpty(siteName))
                throw new TaskExecutionException("Site name missing!");
            if (string.IsNullOrEmpty(protocol))
                throw new TaskExecutionException("Protocol missing!");
            ServerManager oIisMgr = new ServerManager();
            Site oSite = oIisMgr.Sites[siteName];

            //See if this binding is already on some site
            if (oIisMgr.Sites
                .Where(st => st.Bindings.Where(b => b.Protocol == protocol).Any())
                .Any())
            {
                context.WriteInfo("Binding for protocol '{0}' already exists! Doing nothing.", protocol);
                return;
            }

            Binding oBinding = oSite.Bindings.CreateElement();
            oBinding.Protocol = protocol;
            oSite.Bindings.Add(oBinding);

            oIisMgr.CommitChanges();
        }
    }
}
