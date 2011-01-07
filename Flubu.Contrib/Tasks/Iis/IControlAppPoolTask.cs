namespace Flubu.Tasks.Iis
{
    public interface IControlAppPoolTask : ITask
    {
        string ApplicationPoolName { get; set; }
        ControlApplicationPoolAction Action { get; set; }
        bool FailIfNotExist { get; set; }
    }
}