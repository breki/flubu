using System;
using System.Collections.Generic;

namespace Flubu
{
    public class TaskContext : ITaskContext
    {
        public TaskContext(ITaskContextProperties properties)
        {
            this.properties = properties;
        }

        public ITaskContextProperties Properties
        {
            get { return properties; }
        }

        public TaskContext AddLogger (ILogger logger)
        {
            loggers.Add(logger);
            return this;
        }

        public void IncreaseDepth()
        {
            executionDepth++;
        }

        public void WriteMessage(TaskMessageLevel level, string message)
        {
            foreach (ILogger logger in loggers)
            {
                try
                {
                    logger.WriteMessage(level, executionDepth, message);
                }
                catch
                {
                }
            }
        }

        public void DecreaseDepth()
        {
            executionDepth--;
        }

        public void Fail(string message)
        {
            WriteMessage(TaskMessageLevel.Error, message);
            throw new InvalidOperationException(message);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (false == disposed)
            {
                // clean native resources         

                if (disposing)
                {
                    // clean managed resources
                    foreach (ILogger logger in loggers)
                        logger.Dispose();
                }

                disposed = true;
            }
        }

        private int executionDepth;
        private bool disposed;
        private List<ILogger> loggers = new List<ILogger>();
        private ITaskContextProperties properties;
    }
}