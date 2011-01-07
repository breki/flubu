using System;

namespace Flubu
{
    /// <summary>
    /// A base abstract class from which tasks can be implemented.
    /// </summary>
    public abstract class TaskBase : ITask
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public abstract string Description { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is safe to execute in dry run mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is safe to execute in dry run mode; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSafeToExecuteInDryRun
        {
            get { return false; }
        }

        /// <summary>
        /// Executes the task using the specified script execution environment.
        /// </summary>
        /// <remarks>This method implements the basic reporting and error handling for
        /// classes which inherit the <see cref="TaskBase"/> class.</remarks>
        /// <param name="context">The script execution environment.</param>
        public void Execute (ITaskContext context)
        {
            if (context  == null)
                throw new ArgumentNullException ("context");

            context.WriteInfo(DescriptionForLog);
            context.IncreaseDepth();

            try
            {
                DoExecute (context);
            }
            finally
            {
                context.DecreaseDepth();
            }

            //context.LogTaskFinished();
        }

        protected virtual string DescriptionForLog
        {
            get { return Description; }
        }

        /// <summary>
        /// Abstract method defining the actual work for a task.
        /// </summary>
        /// <remarks>This method has to be implemented by the inheriting task.</remarks>
        /// <param name="context">The script execution environment.</param>
        protected abstract void DoExecute (ITaskContext context);
    }
}
