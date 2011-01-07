using System;

namespace Flubu
{
    public interface ITaskContext : IDisposable
    {
        ITaskContextProperties Properties { get; }

        void DecreaseDepth();
        void Fail(string message);
        void IncreaseDepth();
        void ResetDepth();
        void WriteMessage(TaskMessageLevel level, string message);
    }
}
