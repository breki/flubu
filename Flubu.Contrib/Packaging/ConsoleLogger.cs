using System;

namespace Flubu.Packaging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string format, params object[] args)
        {
            Console.Out.WriteLine(format, args);
        }
    }
}