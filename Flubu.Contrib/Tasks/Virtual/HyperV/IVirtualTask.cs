using System;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    /// Completion task interface definition.
    /// </summary>
    public interface IVirtualTask
    {
        /// <summary>
        /// Gets a value indicating whether the task has already finished, false if task is in progress.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        ///   Gets task completion percentage.
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