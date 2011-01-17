using System;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Hyper-v Job states. Suspended
    /// </summary>
    public static class JobState
    {
        [CLSCompliant(false)] public const UInt16 Completed = 7;
        [CLSCompliant(false)] public const UInt16 Exception = 10;
        [CLSCompliant(false)] public const UInt16 Killed = 9;
        [CLSCompliant(false)] public const UInt16 New = 2;
        [CLSCompliant(false)] public const UInt16 Running = 4;
        [CLSCompliant(false)] public const UInt16 Service = 11;
        [CLSCompliant(false)] public const UInt16 ShuttingDown = 6;
        [CLSCompliant(false)] public const UInt16 Starting = 3;
        [CLSCompliant(false)] public const UInt16 Suspended = 5;
        [CLSCompliant(false)] public const UInt16 Terminated = 8;
    }
}