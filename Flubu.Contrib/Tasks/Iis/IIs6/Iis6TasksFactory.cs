namespace Flubu.Tasks.Iis.Iis6
{
    public class Iis6TasksFactory : IIisTasksFactory
    {
        public IControlAppPoolTask ControlAppPoolTask
        {
            get { return new Iis6ControlAppPoolTask(); }
        }

        public ICreateAppPoolTask CreateAppPoolTask
        {
            get { return new Iis6CreateAppPoolTask(); }
        }

        public ICreateWebApplicationTask CreateApplicationTask
        {
            get { return new Iis6CreateWebApplicationTask(); }
        }

        public IDeleteAppPoolTask DeleteAppPoolTask
        {
            get { return new Iis6DeleteAppPoolTask(); }
        }
    }
}