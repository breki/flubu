using System;

namespace Flubu.Builds.Tasks.VersioningTasks
{
    public interface IFetchBuildVersionTask : ITask
    {
        Version BuildVersion { get; }
    }
}