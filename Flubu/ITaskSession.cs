using System;
using System.Diagnostics;

namespace Flubu
{
    public interface ITaskSession : ITaskContext
    {
        bool HasFailed { get; }
        Stopwatch BuildStopwatch { get; }

        void Start(Action<ITaskSession> onFinishDo);
        void Complete();
    }
}