using System;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Completition task interface definition.
    /// </summary>
    public interface IVirtualTask
    {
        /// <summary>
        ///   True if task has already finished, false if task is in progress.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        ///   Gets task completition percentage.
        /// </summary>
        int PercentComplete { get; }

        /// <summary>
        ///   Wait current task to complete.
        /// </summary>
        /// <param name = "timeout">Time to wait</param>
        /// <returns>True if task was completed, false if timeout was reached.</returns>
        bool WaitForCompletion(TimeSpan timeout);
    }
}