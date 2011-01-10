using System;
using System.Diagnostics;
using Flubu.Targeting;

namespace Flubu
{
    public interface ITaskSession : ITaskContext
    {
        bool HasFailed { get; }
        Stopwatch BuildStopwatch { get; }
        TargetTree TargetTree { get; }

        void Start(Action<ITaskSession> onFinishDo);
        void Complete();
    }
}