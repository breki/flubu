using System;

namespace Flubu
{
    public interface ITaskContext : IDisposable
    {
        ITaskContextProperties Properties { get; }

        void IncreaseDepth();
        void WriteMessage(TaskMessageLevel level, string message);
        void DecreaseDepth();
        void Fail(string message);
    }
}
