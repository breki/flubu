namespace Flubu.Tasks.Iis
{
    public interface IIisMaster
    {
        IIisTasksFactory Iis7TasksFactory { get; }       
        IIisTasksFactory LocalIisTasksFactory { get; }
    }
}