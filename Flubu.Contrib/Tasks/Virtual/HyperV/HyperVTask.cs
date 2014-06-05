using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Threading;

namespace Flubu.Tasks.Virtual.HyperV
{
    /// <summary>
    ///   Hyper-v server task class.
    /// </summary>
    public class HyperVTask : IVirtualTask, IDisposable
    {
        public HyperVTask(ManagementBaseObject outParameters, ManagementScope scope)
        {
            if (outParameters == null) throw new ArgumentNullException("outParameters");
            var jobPath = (string)outParameters["Job"];
            task = new ManagementObject(scope, new ManagementPath(jobPath), null);
            //Try to get storage job information
            task.Get();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperVTask"/> class with a specified job return code.
        /// </summary>
        /// <param name="returnCode">Job return code.
        /// </param>
        [CLSCompliant(false)]
        public HyperVTask(UInt32 returnCode)
        {
            this.returnCode = returnCode;
        }

        /// <summary>
        ///   Wait current task to complete.
        /// </summary>
        /// <param name = "timeout">Time to wait</param>
        /// <returns>True if task was completed, false if timeout was reached.</returns>
        public bool WaitForCompletion(TimeSpan timeout)
        {
            if (returnCode.HasValue)
                return true;

            task.Get();
            var jobState = (UInt16)task["JobState"];
            Stopwatch timer = Stopwatch.StartNew();
            while (jobState == JobState.Starting || jobState == JobState.Running)
            {
                Thread.Sleep(1000);
                task.Get();
                jobState = (UInt16)task["JobState"];
                if (timer.Elapsed >= timeout)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the task has already finished, false if task is in progress.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (returnCode.HasValue)
                    return true;

                task.Get();
                return (UInt16)task["JobState"] == JobState.Completed;
            }
        }

        /// <summary>
        ///   Gets task completed percentage.
        /// </summary>
        public int PercentComplete
        {
            get
            {
                if (returnCode.HasValue)
                    return 100;

                task.Get();
                return (int)task["PercentComplete"];
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework",
            MessageId = "System.Management.ManagementObject.#Dispose()")]
        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (task != null)
            {
                task.Dispose();
            }
        }

        private readonly UInt32? returnCode;
        private readonly ManagementObject task;
    }
}