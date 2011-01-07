namespace Flubu.Tasks.Iis
{
    public enum ControlApplicationPoolAction
    {
        /// <summary>
        /// Start the application pool.
        /// </summary>
        Start,

        /// <summary>
        /// Stop the application pool.
        /// </summary>
        Stop,

        /// <summary>
        /// Recycle the application pool.
        /// </summary>
        Recycle,
    }
}