namespace Flubu.Tasks.Iis
{
    public interface ICreateAppPoolTask : ITask
    {
        string ApplicationPoolName { get; set; }

        bool ClassicManagedPipelineMode { get; set; }
        
        CreateApplicationPoolMode Mode { get; set; }
    }
}