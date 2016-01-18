using Flubu.Tasks.Iis;
using NUnit.Framework;

namespace Flubu.Tests
{
    [TestFixture]
    [Ignore("Can only be run on IIS7")]
    public class Iis7Tests
    {
        [Test]
        [Ignore("Need admin rights!")]
        public void AddHttpsBindingToIss()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]); 
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            controlWebsiteTask
                .SiteName("Default Web Site")
                .AddBinding("https")
                .Execute(context);
        }

        [Test]
        [ExpectedException(typeof(TaskExecutionException))]
        public void AddHttpsBindingToIssNullSite()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            controlWebsiteTask
                .AddBinding("https")
                .Execute(context);
        }

        [Test]
        [ExpectedException(typeof(TaskExecutionException))]
        public void AddHttpsBindingToIssEmptySite()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            controlWebsiteTask
                .SiteName("")
                .AddBinding("https")
                .Execute(context);
        }

        [Test]
        [ExpectedException(typeof(TaskExecutionException))]
        public void AddHttpsBindingToIssNullProtocol()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            controlWebsiteTask
                .SiteName("Default Web Site")
                .Execute(context);
        }

        [Test]
        [ExpectedException(typeof(TaskExecutionException))]
        public void AddHttpsBindingToIssEmptyProtocol()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            controlWebsiteTask
                .SiteName("Default Web Site")
                .AddBinding("")
                .Execute(context);
        }

        [Test, Explicit("Needs admin rights.")]
        public void CreateWebSiteTest()
        {
            TaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.Iis7TasksFactory;
            var task = factory.CreateWebSiteTask;
            var mimeType = new MimeType
                               {
                                   FileExtension = "swg",
                                   Type = "Application/text"
                               };
            task
                .WebSiteName("Test")
                .BindingProtocol("https")
                .Port(2443)
                .PhysicalPath("d:\\")
                .AddMimeType(mimeType)
               .Execute(context);
        }

        [Test]
        [ExpectedException(typeof(TaskExecutionException))]
        public void CreateWebSiteNoWebSiteNameTest()
        {
            TaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.Iis7TasksFactory;
            var task = factory.CreateWebSiteTask;
            task.Execute(context);
        }
    }
}
