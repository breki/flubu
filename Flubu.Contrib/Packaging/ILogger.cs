namespace Flubu.Packaging
{
    public interface ILogger
    {
        void Log(string format, params object[] args);
    }
}