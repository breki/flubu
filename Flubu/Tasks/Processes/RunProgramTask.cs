using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Flubu.Tasks.Processes
{
    public class RunProgramTask : TaskBase
    {
        private readonly bool ignoreExitCodes;
        private readonly List<string> programArgs = new List<string>();
        private readonly string programExePath;
        private bool encloseInQuotes;
        private ITaskContext internalContext;
        private int lastExitCode;
        private string workingDirectory = ".";
        private TimeSpan executionTimeout;

        public RunProgramTask(string programExePath)
        {
            this.programExePath = programExePath;
            encloseInQuotes = true;
        }

        public RunProgramTask(string programExePath, bool ignoreExitCodes)
        {
            this.programExePath = programExePath;
            this.ignoreExitCodes = ignoreExitCodes;
            encloseInQuotes = true;
        }

        public override string Description
        {
            get { return "Run program"; }
        }

        /// <summary>
        /// Gets the exit code of the last external program that was run by the runner.
        /// </summary>
        /// <value>The exit code of the last external program.</value>
        public int LastExitCode
        {
            get { return lastExitCode; }
        }

        /// <summary>
        /// Set the execution timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>This instance</returns>
        public RunProgramTask ExecutionTimeout(TimeSpan timeout)
        {
            executionTimeout = timeout;
            return this;
        }

        public RunProgramTask EncloseParametersInQuotes(bool enclose)
        {
            encloseInQuotes = enclose;
            return this;
        }

        public RunProgramTask AddArgument(string argument)
        {
            programArgs.Add(argument);
            return this;
        }

        public RunProgramTask AddArgument(string format, params object[] args)
        {
            programArgs.Add(string.Format(CultureInfo.InvariantCulture, format, args));
            return this;
        }

        public RunProgramTask SetWorkingDir(string fullPath)
        {
            workingDirectory = fullPath;
            return this;
        }

        public RunProgramTask UseProgramDirAsWorkingDir()
        {
            workingDirectory = null;
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            internalContext = context;
            string formatString = encloseInQuotes ? "\"{0}\" " : "{0} ";
            using (var process = new Process())
            {
                var argumentLineBuilder = new StringBuilder();
                foreach (string programArg in programArgs)
                    argumentLineBuilder.AppendFormat(formatString, programArg);

                var processStartInfo = new ProcessStartInfo(programExePath, argumentLineBuilder.ToString())
                                           {
                                               CreateNoWindow = true,
                                               ErrorDialog = false,
                                               RedirectStandardError = true,
                                               RedirectStandardOutput = true,
                                               UseShellExecute = false,
                                               WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(programExePath)
                                           };

                context.WriteInfo(
                    "Running program '{0}' (work. dir='{1}', args = '{2}')",
                    programExePath,
                    processStartInfo.WorkingDirectory,
                    argumentLineBuilder);

                process.StartInfo = processStartInfo;
                process.ErrorDataReceived += ProcessErrorDataReceived;
                process.OutputDataReceived += ProcessOutputDataReceived;
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (executionTimeout == TimeSpan.MinValue)
                    process.WaitForExit();
                else
                    process.WaitForExit(executionTimeout.Milliseconds);

                context.WriteInfo("Exit code: {0}", process.ExitCode);

                lastExitCode = process.ExitCode;

                if (false == ignoreExitCodes && process.ExitCode != 0)
                    context.Fail("Program '{0}' returned exit code {1}.", programExePath, process.ExitCode);
            }
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            internalContext.WriteError(e.Data);
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            internalContext.WriteMessage(TaskMessageLevel.Debug, e.Data);
        }
    }
}