namespace Flubu.Tasks.Virtual.HyperV
{
    public enum StateChange
    {
        None = 0,
        TurnOn = 2,
        Turnoff = 3,
        Reboot = 10,
        Reset = 11,
        Pause = 32768,
        Suspend = 32769
    }
}