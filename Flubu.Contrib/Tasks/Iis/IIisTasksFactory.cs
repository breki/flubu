namespace Flubu.Tasks.Iis
{
    public interface IIisTasksFactory
    {
        ICreateWebsiteTask CreateWebsiteTask { get; }
        ICreateWebApplicationTask CreateApplicationTask { get; }
        IControlAppPoolTask ControlAppPoolTask { get; }
        ICreateAppPoolTask CreateAppPoolTask { get; }
        IDeleteAppPoolTask DeleteAppPoolTask { get; }
        IAddWebsiteBindingTask AddWebsiteBindingTask { get; }
    }
}