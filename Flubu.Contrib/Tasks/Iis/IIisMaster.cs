namespace Flubu.Tasks.Iis
{
    public interface IIisMaster
    {
        IIisTasksFactory Iis6TasksFactory { get; }

        IIisTasksFactory Iis7TasksFactory { get; }
        
        IIisTasksFactory LocalIisTasksFactory { get; }
    }
}