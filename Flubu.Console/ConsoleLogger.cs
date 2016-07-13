using System.Globalization;

namespace Flubu.Console
{
    public class ConsoleLogger : IConsoleLogger
    {
        public void Log(string message, params object[] args)
        {
            System.Console.Out.WriteLine(
                string.Format(CultureInfo.InvariantCulture, message, args));
        }
    }
}