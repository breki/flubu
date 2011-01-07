using System;
using System.Diagnostics;

namespace Flubu
{
    public class TaskSession : ITaskSession
    {
        public TaskSession()
        {
            hasFailed = true;
            buildStopwatch.Start();
        }

        public void Start(ITaskContext taskContext)
        {
            this.taskContext = taskContext;
            hasFailed = true;
            buildStopwatch.Start();
        }

        /// <summary>
        /// Marks the runner as having completed its work sucessfully. This is the last method
        /// that should be called on the runner before it gets disposed.
        /// </summary>
        public void Complete()
        {
            hasFailed = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">If <code>false</code>, cleans up native resources. 
        /// If <code>true</code> cleans up both managed and native resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                    buildStopwatch.Stop();
                    //taskContext.LogRunnerFinished(this);

                    Beeper.Beep(hasFailed ? MessageBeepType.Error : MessageBeepType.Ok);
                }

                disposed = true;
            }
        }

        private Stopwatch buildStopwatch = new Stopwatch();
        private bool disposed;
        private bool hasFailed;
        private ITaskContext taskContext;
    }
}