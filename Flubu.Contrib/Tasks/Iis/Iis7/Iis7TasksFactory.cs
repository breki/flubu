using System;

namespace Flubu.Tasks.Iis.Iis7
{
    public class Iis7TasksFactory : IIisTasksFactory
    {
        public IControlAppPoolTask ControlAppPoolTask
        {
            get { return new Iis7ControlAppPoolTask(); }
        }

        public ICreateAppPoolTask CreateAppPoolTask
        {
            get { return new Iis7CreateAppPoolTask(); }
        }

        public ICreateWebsiteTask CreateWebsiteTask
        {
            get { return new Iis7CreateWebsiteTask(); }
        }

        public ICreateWebApplicationTask CreateApplicationTask
        {
            get { return new Iis7CreateWebApplicationTask(); }
        }

        public IDeleteAppPoolTask DeleteAppPoolTask
        {
            get { return new Iis7DeleteAppPoolTask(); }
        }

        public IAddWebsiteBindingTask AddWebsiteBindingTask
        {
            get { return new Iis7AddWebsiteBindingTask(); }
        }
    }
}