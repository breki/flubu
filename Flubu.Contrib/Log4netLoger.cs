using System;
using log4net;

namespace Flubu
{
    public class Log4NetLogger : ILogger
    {
        public Log4NetLogger(string name)
        {
           log = LogManager.GetLogger(name);
        }
        
        /// <summary>
        ///                     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void WriteMessage(TaskMessageLevel level, int depth, string message)
        {
            switch (level)
            {
                case TaskMessageLevel.Debug:
                    log.Debug(message);
                    break;
                case TaskMessageLevel.Info:
                    log.Info(message);
                    break;
                case TaskMessageLevel.Warn:
                    log.Warn(message);
                    break;
                case TaskMessageLevel.Error:
                    log.Error(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }
        }

        //public void LogRunnerFinished(IFlubuRunner runner)
        //{
        //    // reset the depth counter to make the build report non-indented
        //    executionDepthCounter = 0;

        //    if (runner.HasFailed)
        //        log.Error("BUILD FAILED");
        //    else
        //        log.Info("BUILD SUCCESSFUL");

        //    TimeSpan buildDuration = runner.BuildStopwatch.Elapsed;
        //    log.InfoFormat(CultureInfo.InvariantCulture, "Build finish time: {0:g}", DateTime.Now);
        //    log.InfoFormat(
        //        CultureInfo.InvariantCulture, 
        //        "Build duration: {0:D2}:{1:D2}:{2:D2} ({3:d} seconds)",
        //        buildDuration.Hours,
        //        buildDuration.Minutes,
        //        buildDuration.Seconds,
        //        (int)buildDuration.TotalSeconds);
        //}

        //public void LogTargetFinished(ITarget target)
        //{
        //    log.InfoFormat(
        //        CultureInfo.InvariantCulture,
        //        "{0} finished (took {1} seconds)",
        //        target.TargetName,
        //        (int)target.TargetStopwatch.Elapsed.TotalSeconds);
        //    executionDepthCounter--;
        //}

        //public void LogTargetStarted(ITarget target)
        //{
        //    log.InfoFormat(CultureInfo.InvariantCulture, "{0}:", target.TargetName);
        //    executionDepthCounter++;
        //}

        //public void LogTaskFinished()
        //{
        //    executionDepthCounter--;
        //}

        //public void LogTaskStarted(string taskDescription)
        //{
        //    log.InfoFormat(CultureInfo.InvariantCulture, "{0}", taskDescription);
        //    executionDepthCounter++;
        //}

        protected virtual void Dispose(bool disposing)
        {
            if (false == disposed)
            {
                if (disposing)
                {
                   // add some disposing code if needed
                }

                disposed = true;
            }
        }

        private bool disposed;
        private static ILog log;
    }
}
