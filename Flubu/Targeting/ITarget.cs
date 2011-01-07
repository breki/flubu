using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Flubu.Targeting
{
    public interface ITarget : ITask
    {
        ICollection<string> Dependencies { get; }

        string TargetName { get; }

        Stopwatch TargetStopwatch { get; }

        /// <summary>
        /// Gets a value indicating whether this target is hidden. Hidden targets will not be
        /// visible in the list of targets displayed to the user as help.
        /// </summary>
        /// <value><c>true</c> if this target is hidden; otherwise, <c>false</c>.</value>
        bool IsHidden { get; }

        /// <summary>
        /// Specifies targets on which this target depends on.
        /// </summary>
        /// <param name="targetNames">The dependency target names.</param>
        /// <returns>This same instance of <see cref="ITarget"/>.</returns>
        ITarget DependsOn(params string[] targetNames);

        ITarget Do (Action<ITaskContext> targetAction);

        /// <summary>
        /// Sets the target as the default target for the runner.
        /// </summary>
        /// <returns>This same instance of <see cref="ITarget"/>.</returns>
        ITarget SetAsDefault();

        ITarget SetDescription(string description);

        /// <summary>
        /// Sets the target as hidden. Hidden targets will not be
        /// visible in the list of targets displayed to the user as help.
        /// </summary>
        /// <returns>This same instance of <see cref="FlubuRunnerTarget{TRunner}"/>.</returns>
        ITarget SetAsHidden();
    }
}