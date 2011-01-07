namespace Flubu.Tasks.Iis
{
    public interface IIisTasksFactory
    {
        ICreateWebApplicationTask CreateApplicationTask { get; }
        IControlAppPoolTask ControlAppPoolTask { get; }
        ICreateAppPoolTask CreateAppPoolTask { get; }
        IDeleteAppPoolTask DeleteAppPoolTask { get; }
    }
}