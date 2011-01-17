using System;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Hyper-V Job return codes.
    /// </summary>
    public static class ReturnCode
    {
        [CLSCompliant(false)] public const UInt32 AccessDenied = 32769;
        [CLSCompliant(false)] public const UInt32 Completed = 0;
        [CLSCompliant(false)] public const UInt32 Failed = 32768;
        [CLSCompliant(false)] public const UInt32 IncorrectDataType = 32776;
        [CLSCompliant(false)] public const UInt32 InvalidParameter = 32773;
        [CLSCompliant(false)] public const UInt32 InvalidState = 32775;
        [CLSCompliant(false)] public const UInt32 NotSupported = 32770;
        [CLSCompliant(false)] public const UInt32 OutOfMemory = 32778;
        [CLSCompliant(false)] public const UInt32 Started = 4096;
        [CLSCompliant(false)] public const UInt32 SystemInUser = 32774;
        [CLSCompliant(false)] public const UInt32 SystemNotAvailable = 32777;
        [CLSCompliant(false)] public const UInt32 Timeout = 32772;
        [CLSCompliant(false)] public const UInt32 Unknown = 32771;
    }
}