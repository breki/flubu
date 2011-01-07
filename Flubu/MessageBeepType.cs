namespace Flubu
{
    public enum MessageBeepType
    {
        /// <summary>
        /// Default message beep.
        /// </summary>
        Default = -1,

        /// <summary>
        /// OK message beep.
        /// </summary>
        Ok = 0x00000000,

        /// <summary>
        /// Error message beep.
        /// </summary>
        Error = 0x00000010,

        /// <summary>
        /// Question message beep.
        /// </summary>
        Question = 0x00000020,

        /// <summary>
        /// Warning message beep.
        /// </summary>
        Warning = 0x00000030,

        /// <summary>
        /// Information message beep.
        /// </summary>
        Information = 0x00000040,
    }
}