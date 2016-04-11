using System;
using System.Collections.ObjectModel;
using Flubu.Services;

namespace Flubu.Tasks.Processes
{
    public interface IExternalProcessTask<out T> where T : TaskBase
    {
        Collection<string> Parameters { get; }
        T WithParams(params string[] parameters);
        T AddParam(string param);
        T AddParam(string format, params object[] args);

        string WorkFolder { get; }
        T WorkingFolder(string folder);

        TimeSpan? Timeout { get; }
        T ExecutionTimeout(TimeSpan timeout);

        T TasksFactory(ICommonTasksFactory factory);
        ICommonTasksFactory CommonTasksFactory { get; }
    }
}
