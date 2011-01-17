using System.Diagnostics.CodeAnalysis;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Virtual machine states.
    /// </summary>
    public enum VirtualMachineState
    {
        Unknown = 0,
        Enabled = 2,
        Disabled = 3,
        Paused = 32768,
        Suspended = 32769,
        Starting = 32770,
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Snapshotting")] Snapshotting = 32771,
        Migrating = 32772,
        Saving = 32773,
        Stopping = 32774,
        Deleted = 32775,
        Pausing = 32776,
        Resuming = 32777
    }
}