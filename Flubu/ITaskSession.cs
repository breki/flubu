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

        /// <summary>
        /// Resets the task session, cleaning all information about the previously executed targets. 
        /// </summary>
        void Reset();

        void Complete();
    }
}