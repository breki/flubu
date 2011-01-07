using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Flubu.Targeting;

namespace Flubu
{
    /// <summary>
    /// A standard multi-colored console output for Flubu.
    /// </summary>
    public class MulticoloredConsoleLogger : ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MulticoloredConsoleLogger"/> class
        /// using the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer to use to write log messages.</param>
        public MulticoloredConsoleLogger(TextWriter writer)
        {
            this.writer = writer;
            stopwatch.Start();
            lastTimeMark = TimeSpan.Zero;

            if (IsConsoleOutput)
            {
                // remember user's standard colors so he doesn't get pissed off 
                // if we left the console switched to pink after the build
                defaultForegroundColor = Console.ForegroundColor;
                defaultBackgroundColor = Console.BackgroundColor;

                Console.BackgroundColor = ConsoleColor.Black;
            }
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

        public void WriteMessage(TaskMessageLevel level, int depth, string message)
        {
            ConsoleColor color;
            switch (level)
            {
                case TaskMessageLevel.Debug:
                    color = ConsoleColor.DarkGray;
                    break;
                case TaskMessageLevel.Info:
                    color = ConsoleColor.Gray;
                    break;
                case TaskMessageLevel.Warn:
                case TaskMessageLevel.Error:
                    color = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("level");
            }

            WriteLine(color, depth, message);
        }

        //public void LogRunnerFinished(IFlubuRunner runner)
        //{
        //    // reset the depth counter to make the build report non-indented
        //    executionDepthCounter = 0;

        //    LogTargetDurations(runner);

        //    WriteLine (ConsoleColor.DarkGray, String.Empty);
        //    if (runner.HasFailed)
        //        WriteLine(ConsoleColor.Red, "BUILD FAILED");
        //    else
        //        WriteLine(ConsoleColor.Green, "BUILD SUCCESSFUL");

        //    TimeSpan buildDuration = runner.BuildStopwatch.Elapsed;
        //    WriteLine(ConsoleColor.White, "Build finish time: {0:g}", DateTime.Now);
        //    WriteLine(
        //        ConsoleColor.White, 
        //        "Build duration: {0:D2}:{1:D2}:{2:D2} ({3:d} seconds)", 
        //        buildDuration.Hours, 
        //        buildDuration.Minutes, 
        //        buildDuration.Seconds,
        //        (int)buildDuration.TotalSeconds);
        //}

        //public void LogTargetFinished(ITarget target)
        //{
        //    executionDepthCounter--;
        //    WriteLine(
        //        ConsoleColor.White, 
        //        "{0} finished (took {1} seconds)", 
        //        target.TargetName,
        //        (int)target.TargetStopwatch.Elapsed.TotalSeconds);
        //}

        //public void LogTargetStarted(ITarget target)
        //{
        //    WriteLine(ConsoleColor.White, String.Empty);
        //    WriteLine (ConsoleColor.White, "{0}:", target.TargetName);
        //    executionDepthCounter++;
        //}

        //public void LogTaskFinished()
        //{
        //    executionDepthCounter--;
        //}

        //public void LogTaskStarted(string taskDescription)
        //{
        //    WriteLine (ConsoleColor.Gray, "{0}", taskDescription);
        //    executionDepthCounter++;
        //}

        /// <summary>
        /// Gets a value indicating whether this logger logs to the <see cref="Console.Out"/>.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance uses console output; otherwise, <c>false</c>.
        /// </value>
        protected bool IsConsoleOutput
        {
            get { return ReferenceEquals(writer, System.Console.Out); }
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
                    // restore user's standard colors
                    Console.ForegroundColor = defaultForegroundColor;
                    Console.BackgroundColor = defaultBackgroundColor;

                    writer.Dispose();
                }

                disposed = true;
            }
        }

        private void WriteLine (
            ConsoleColor foregroundColor,
            int depth,
            string message)
        {
            if (IsConsoleOutput)
                Console.ForegroundColor = foregroundColor;

            WriteTimeMark();

            string indentation = new string(' ', depth * 3);
            writer.Write (indentation);
            writer.WriteLine (message);
        }

        private void WriteLine(
            ConsoleColor foregroundColor, 
            int depth,
            string format, 
            params object[] args)
        {
            WriteTimeMark();

            if (IsConsoleOutput)
                Console.ForegroundColor = foregroundColor;

            string indentation = new string(' ', depth * 3);
            writer.Write(indentation);
            writer.WriteLine(format, args);
        }

        private void WriteTimeMark()
        {
            TimeSpan timeMark = stopwatch.Elapsed;
            TimeSpan diff = timeMark - lastTimeMark;
            if (diff.TotalSeconds >= 2)
            {
                if (IsConsoleOutput)
                    Console.ForegroundColor = ConsoleColor.Magenta;
                writer.Write("{0,3}:", (int)diff.TotalSeconds);
            }
            else
                writer.Write("    ");
            lastTimeMark = timeMark;
        }

        private bool disposed;
        private readonly TextWriter writer;
        private ConsoleColor defaultForegroundColor;
        private ConsoleColor defaultBackgroundColor;
        private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan lastTimeMark;
    }
}