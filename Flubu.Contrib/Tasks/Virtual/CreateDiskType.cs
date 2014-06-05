namespace Flubu.Tasks.Virtual
{
    /// <summary>
    /// Various disk types for hyper-v image creation.
    /// </summary>
    public enum CreateDiskType
    {
        /// <summary>
        /// Create fixed disk. File will be pre-allocated with maximum allowed size.
        /// </summary>
        Fixed,

        /// <summary>
        /// Create dynamically expanding disk image.
        /// </summary>
        Dynamic,

        /// <summary>
        /// Create differencing disk.
        /// </summary>
        Differencing
    }
}