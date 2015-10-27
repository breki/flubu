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
            programArgs.Add(new Arg(argument));
            return this;
        }

        public RunProgramTask AddArgument(string format, params object[] args)
        {
            Arg arg = new Arg (string.Format (CultureInfo.InvariantCulture, format, args));
            programArgs.Add(arg);
            return this;
        }

        public RunProgramTask AddSecureArgument(string argument)
        {
            programArgs.Add(new Arg(argument, true));
            return this;
        }

        public RunProgramTask AddSecureArgument(string format, params object[] args)
        {
            Arg arg = new Arg (string.Format (CultureInfo.InvariantCulture, format, args), true);
            programArgs.Add(arg);
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
            using (Process process = new Process())
            {
                StringBuilder argumentLineBuilder = new StringBuilder();
                StringBuilder argumentLineLogBuilder = new StringBuilder();
                foreach (Arg programArg in programArgs)
                {
                    argumentLineBuilder.AppendFormat(formatString, programArg.ToRawString());
                    argumentLineLogBuilder.AppendFormat(formatString, programArg.ToSecureString());
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo(programExePath, argumentLineBuilder.ToString())
                                           {
                                               CreateNoWindow = true,
                                               ErrorDialog = false,
                                               RedirectStandardError = true,
                                               RedirectStandardOutput = true,
                                               UseShellExecute = false,
                                               WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(programExePath)
                                           };
                
                int timeout = executionTimeout.Milliseconds;

                context.WriteInfo(
                    "Running program '{0}':(work. dir='{1}', args = '{2}', timeout = {3})",
                    programExePath,
                    processStartInfo.WorkingDirectory,
                    argumentLineLogBuilder,
                    timeout <= 0 ? "infinite" : executionTimeout.Milliseconds.ToString(CultureInfo.InvariantCulture));

                process.StartInfo = processStartInfo;
                process.ErrorDataReceived += ProcessErrorDataReceived;
                process.OutputDataReceived += ProcessOutputDataReceived;
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (timeout<=0)
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

        private ITaskContext internalContext;
        private readonly bool ignoreExitCodes;
        private int lastExitCode;
        private readonly string programExePath;
        private readonly List<Arg> programArgs = new List<Arg>();
        private string workingDirectory = ".";
        private bool encloseInQuotes;
        private TimeSpan executionTimeout = TimeSpan.MinValue;

        private class Arg
        {
            public Arg(string value, bool isSecure = false)
            {
                this.value = value;
                this.isSecure = isSecure;
            }

            public string ToRawString()
            {
                return value;
            }

            public string ToSecureString()
            {
                return isSecure ? "<hidden>" : value;
            }

            public override string ToString()
            {
                return "Do not use this method";
            }

            private readonly string value;
            private readonly bool isSecure;
        }
    }
}