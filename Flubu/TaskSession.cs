using System;
using System.Diagnostics;

namespace Flubu
{
    public class TaskSession : TaskContext, ITaskSession
    {
        public TaskSession(ITaskContextProperties taskContextProperties) : base(taskContextProperties)
        {
            hasFailed = true;
            buildStopwatch.Start();
        }

        public Stopwatch BuildStopwatch
        {
            get { return buildStopwatch; }
        }

        public bool HasFailed
        {
            get { return hasFailed; }
        }

        public void Start(Action<ITaskSession> onFinishDo)
        {
            this.onFinishDo = onFinishDo;
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

        protected override void Dispose(bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                    buildStopwatch.Stop();

                    if (onFinishDo != null)
                        onFinishDo(this);
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }

        private Stopwatch buildStopwatch = new Stopwatch();
        private bool disposed;
        private bool hasFailed;
        private Action<ITaskSession> onFinishDo;
    }
}