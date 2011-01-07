using System;

namespace Flubu
{
    public interface ITaskSession : IDisposable
    {
        void Start(ITaskContext taskContext);
        void Complete();
    }
}