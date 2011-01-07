namespace Flubu
{
    /// <summary>
    /// Specifies basic properties and methods for a task.
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        string Description { get; }

        /// <summary>
        /// Executes the task using the specified script execution environment.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        void Execute (ITaskContext context);
    }
}
