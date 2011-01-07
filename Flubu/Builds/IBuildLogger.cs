namespace Flubu.Builds
{
    public interface IBuildLogger
    {
        void Log (string format, params object[] args);
    }
}