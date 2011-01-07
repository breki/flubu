namespace Flubu.Tasks.Iis
{
    public enum CreateApplicationPoolMode
    {
        /// <summary>
        /// The task should fail if the application pool already exists.
        /// </summary>
        FailIfAlreadyExists,

        /// <summary>
        /// If the application pool already exists, it should be updated.
        /// </summary>
        UpdateIfExists,

        /// <summary>
        /// If the application pool already exists, the task should do nothing.
        /// </summary>
        DoNothingIfExists,
    }
}