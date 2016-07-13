namespace Flubu.Console
{
    public interface IConsoleLogger
    {
        void Log(string message, params object[] args);
    }
}