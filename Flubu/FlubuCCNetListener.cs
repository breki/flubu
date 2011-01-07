using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Flubu.Targeting;

namespace Flubu
{
    /// <summary>
    /// Custom build listener for Flubu. This listener writes the X last messages to a file
    /// for viewing the build progress in CCNET.
    /// The extra listenfile is XML-based, consisting in the hierarchy of the NAnt project
    /// and the last X messages under the last target of the NAnt project.
    /// </summary>
    /// <remarks>The code is adopted from the custom build listener for NAnt (http://confluence.public.thoughtworks.org/display/CCNETCOMM/Viewing+build+progress+with+Nant+and+MSBuild).
    /// </remarks>
    public class FlubuCCNetListener : ILogger
    {
        public FlubuCCNetListener(string traceFileName)
        {
            this.traceFileName = traceFileName;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        public void LogError(string message)
        {
        }

        public void LogError(string format, params object[] args)
        {
        }

        public void LogMessage(string message)
        {
        }

        public void LogMessage(string format, params object[] args)
        {
        }

        public void LogRunnerFinished(IFlubuRunner runner)
        {
            AddMessageToQueue(runner.HasFailed ? "BUILD FAILED" : "BUILD SUCCESSFUL");
            if (this.projectTargetStack.Count > 0)
                this.projectTargetStack.Pop ();
        }

        public void LogTargetFinished(ITarget target)
        {
            if (this.projectTargetStack.Count > 0)
                this.projectTargetStack.Pop ();
        }

        public void LogTargetStarted(ITarget target)
        {
            this.projectTargetStack.Push(FormatMessage("Starting Target {0}", target.TargetName));
            this.messageQueue.Clear ();
            WriteQueueData();
        }

        public void LogTaskFinished()
        {
        }

        public void LogTaskStarted(string taskDescription)
        {
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">If <code>false</code>, cleans up native resources. 
        /// If <code>true</code> cleans up both managed and native resources</param>
        protected virtual void Dispose (bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                }

                disposed = true;
            }
        }

        private void AddMessageToQueue (string messageData)
        {
            if (this.messageQueue.Count > MessageQueueLength)
                this.messageQueue.Dequeue ();

            this.messageQueue.Enqueue (messageData);
        }

        private string CleanUpMessageForXMLLogging (string msg)
        {
            return msg.Replace ("\"", string.Empty);
        }

        private string FormatMessage (string messageFormat, params object[] args)
        {
            string message = String.Format (
                CultureInfo.InvariantCulture,
                messageFormat,
                args);

            string data = string.Format (
                CultureInfo.InvariantCulture,
                "<Item Time=\"{0}\" Data=\"{1}\" />",
                GetTimeStamp (),
                CleanUpMessageForXMLLogging (message));

            return data;
        }

        private string GetTimeStamp ()
        {
            return System.DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
        }

        private void WriteQueueData ()
        {
            System.Collections.ArrayList projectsTargets;
            projectsTargets = new System.Collections.ArrayList ();

            // empty file
            using (StreamWriter traceFile = new System.IO.StreamWriter (this.traceFileName, false))
            {
                traceFile.AutoFlush = false;
                traceFile.WriteLine("<data>");

                // write hierarchy
                projectsTargets.AddRange(this.projectTargetStack.ToArray());
                projectsTargets.Reverse();

                foreach (string data in projectsTargets)
                    traceFile.WriteLine("{0}", data);

                // write messages
                foreach (string data in this.messageQueue.ToArray())
                    traceFile.WriteLine("{0}", data);

                traceFile.WriteLine("</data>");
            }
        }

        private bool disposed;
        private Queue<string> messageQueue = new Queue<string> ();
        private const int MessageQueueLength = 10;
        private Stack<string> projectTargetStack = new Stack<string> ();
        private string traceFileName;
    }
}