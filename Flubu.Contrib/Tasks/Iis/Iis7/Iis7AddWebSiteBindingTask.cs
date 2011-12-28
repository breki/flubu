using System;
using System.Globalization;
using System.Linq;
using System.Text;
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
        private string certificateStore;
        private string certificateHash;

        public Iis7AddWebsiteBindingTask SiteName(string name)
        {
            siteName = name;
            return this;
        }

        public Iis7AddWebsiteBindingTask AddBinding(string bindingProtocol)
        {
            protocol = bindingProtocol;
            return this;
        }

        public Iis7AddWebsiteBindingTask CertificateStore(string store)
        {
            certificateStore = store;
            return this;
        }

        public Iis7AddWebsiteBindingTask CertificateHash(string hash)
        {
            certificateHash = hash;
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            if(string.IsNullOrEmpty(siteName))
                throw new TaskExecutionException("Site name missing!");
            if (string.IsNullOrEmpty(protocol))
                throw new TaskExecutionException("Protocol missing!");
            if(protocol.IndexOf("https", StringComparison.OrdinalIgnoreCase) >= 0 &&
                (string.IsNullOrEmpty(certificateStore) || string.IsNullOrEmpty(certificateHash)))
            {
                throw new TaskExecutionException("Certificate store or hash not set for SSL protocol");
            }
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
            oBinding.CertificateStoreName = certificateStore;
            oBinding.CertificateHash = Encoding.UTF8.GetBytes(certificateHash);
            oSite.Bindings.Add(oBinding);

            oIisMgr.CommitChanges();
        }
    }
}
