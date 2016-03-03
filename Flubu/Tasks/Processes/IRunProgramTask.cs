using System;
using JetBrains.Annotations;

namespace Flubu.Tasks.Processes
{
    public interface IRunProgramTask : ITask
    {
        /// <summary>
        /// Gets the exit code of the last external program that was run by the runner.
        /// </summary>
        /// <value>The exit code of the last external program.</value>
        int LastExitCode { get; }

        /// <summary>
        /// Set the execution timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>This instance</returns>
        [NotNull]
        IRunProgramTask ExecutionTimeout(TimeSpan timeout);

        [NotNull]
        IRunProgramTask EncloseParametersInQuotes (bool enclose);
        [NotNull]
        IRunProgramTask AddArgument (string argument);
        [NotNull]
        IRunProgramTask AddArgument (string format, params object[] args);
        [NotNull]
        IRunProgramTask AddSecureArgument (string argument);
        [NotNull]
        IRunProgramTask AddSecureArgument (string format, params object[] args);
        [NotNull]
        IRunProgramTask SetWorkingDir (string fullPath);
        [NotNull]
        IRunProgramTask UseProgramDirAsWorkingDir ();
    }
}