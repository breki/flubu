namespace Flubu.Tasks.Virtual
{
    /// <summary>
    /// Various disk types for hyper-v image creation.
    /// </summary>
    public enum CreateDiskType
    {
        /// <summary>
        /// Create fixed disk. File will be prealocated with maximum allowed size.
        /// </summary>
        Fixed,
        /// <summary>
        /// Create dynamicly expanding disk image.
        /// </summary>
        Dynamic,
        /// <summary>
        /// Create diferencing disk.
        /// </summary>
        Differencing
    }
}