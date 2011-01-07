namespace Flubu.Tasks.WindowsServices
{
    /// <summary>
    /// The operating mode for the <see cref="InstallWindowsServiceTask"/> task.
    /// </summary>
    public enum InstallWindowsServiceMode
    {
        /// <summary>
        /// If the Windows service already exists, the task fails.
        /// </summary>
        FailIfAlreadyInstalled,

        /// <summary>
        /// If the Windows service already exists, it is reinstalled.
        /// </summary>
        ReinstallIfExists,

        /// <summary>
        /// If the Windows service already exists, the task does nothing.
        /// </summary>
        DoNothingIfExists,
    }
}