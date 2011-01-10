using System;
using System.Diagnostics;
using Flubu.Targeting;

namespace Flubu
{
    public class TaskSession : TaskContext, ITaskSession
    {
        public TaskSession(ITaskContextProperties taskContextProperties) : base(taskContextProperties)
        {
            hasFailed = true;
            buildStopwatch.Start();
        }

        public TaskSession(ITaskContextProperties taskContextProperties, TargetTree targetTree)
            : base(taskContextProperties)
        {
            hasFailed = true;
            buildStopwatch.Start();
            this.targetTree = targetTree;
        }

        public Stopwatch BuildStopwatch
        {
            get { return buildStopwatch; }
        }

        public TargetTree TargetTree
        {
            get { return targetTree; }
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
        private TargetTree targetTree;
    }
}