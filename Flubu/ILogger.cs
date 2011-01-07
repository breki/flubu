using System;

namespace Flubu
{
    public interface ILogger : IDisposable
    {
        void WriteMessage(TaskMessageLevel level, int depth, string message);
    }
}