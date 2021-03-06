﻿using Flubu.Tasks.Iis;
using NUnit.Framework;

namespace Flubu.Tests.TasksTests
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
        public void AddHttpsBindingToIssNullSite()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            Assert.Throws<TaskExecutionException>(() => controlWebsiteTask.AddBinding("https").Execute(context));
        }

        [Test]
        public void AddHttpsBindingToIssEmptySite()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            Assert.Throws<TaskExecutionException>(() => controlWebsiteTask
                                                            .SiteName(string.Empty)
                                                            .AddBinding("https")
                                                            .Execute(context));
        }

        [Test]
        public void AddHttpsBindingToIssNullProtocol()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            Assert.Throws<TaskExecutionException>(() => controlWebsiteTask
                                                            .SiteName("Default Web Site")
                                                            .Execute(context));
        }

        [Test]
        public void AddHttpsBindingToIssEmptyProtocol()
        {
            ITaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebsiteBindingTask controlWebsiteTask = factory.AddWebsiteBindingTask;
            Assert.Throws<TaskExecutionException>(() => controlWebsiteTask
                                                            .SiteName("Default Web Site")
                                                            .AddBinding(string.Empty)
                                                            .Execute(context));
        }

        [Test, Explicit("Needs admin rights.")]
        public void CreateWebSiteTest()
        {
            TaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.Iis7TasksFactory;
            var task = factory.CreateWebsiteTask;
            var mimeType = new MimeType
                               {
                                   FileExtension = "swg",
                                   MimeTypeName = "Application/text"
                               };
            task
                .WebsiteName("Test")
                .BindingProtocol("https")
                .Port(2443)
                .PhysicalPath("d:\\")
                .AddMimeType(mimeType)
               .Execute(context);
        }

        [Test]
        public void CreateWebSiteNoWebSiteNameTest()
        {
            TaskContext context = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.Iis7TasksFactory;
            var task = factory.CreateWebsiteTask;
            Assert.Throws<TaskExecutionException>(() => task.Execute(context));
        }
    }
}
