using System;

namespace Flubu.Builds.Tasks
{
    public interface IFetchBuildVersionTask : ITask
    {
        Version BuildVersion { get; }
    }
}