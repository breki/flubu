using System;
using System.Diagnostics;
using System.IO;

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

        /// <summary>
        /// Gets a value indicating whether this logger logs to the <see cref="Console.Out"/>.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance uses console output; otherwise, <c>false</c>.
        /// </value>
        protected bool IsConsoleOutput
        {
            get { return ReferenceEquals(writer, Console.Out); }
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
        private readonly ConsoleColor defaultForegroundColor;
        private readonly ConsoleColor defaultBackgroundColor;
        private readonly Stopwatch stopwatch = new Stopwatch();
        private TimeSpan lastTimeMark;
    }
}