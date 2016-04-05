using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Flubu.Services;

namespace Flubu.Tasks.Processes
{
    public class RunProgramTask : TaskBase, IRunProgramTask
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

        public IProcessRunner ProcessRunner
        {
            get { return processRunner; }
            set { processRunner = value; }
        }

        /// <summary>
        /// Set the execution timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>This instance</returns>
        public IRunProgramTask ExecutionTimeout(TimeSpan timeout)
        {
            executionTimeout = timeout;
            return this;
        }

        public IRunProgramTask EncloseParametersInQuotes(bool enclose)
        {
            encloseInQuotes = enclose;
            return this;
        }

        public IRunProgramTask AddArgument(string argument)
        {
            programArgs.Add(new Arg(argument));
            return this;
        }

        public IRunProgramTask AddArgument(string format, params object[] args)
        {
            Arg arg = new Arg (string.Format (CultureInfo.InvariantCulture, format, args));
            programArgs.Add(arg);
            return this;
        }

        public IRunProgramTask AddSecureArgument(string argument)
        {
            programArgs.Add(new Arg(argument, true));
            return this;
        }

        public IRunProgramTask AddSecureArgument(string format, params object[] args)
        {
            Arg arg = new Arg (string.Format (CultureInfo.InvariantCulture, format, args), true);
            programArgs.Add(arg);
            return this;
        }

        public IRunProgramTask SetWorkingDir(string fullPath)
        {
            workingDirectory = fullPath;
            return this;
        }

        public IRunProgramTask UseProgramDirAsWorkingDir()
        {
            workingDirectory = null;
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            internalContext = context;
            string formatString = encloseInQuotes ? "\"{0}\" " : "{0} ";
            
            StringBuilder argumentLineBuilder = new StringBuilder();
            StringBuilder argumentLineLogBuilder = new StringBuilder();
            foreach (Arg programArg in programArgs)
            {
                argumentLineBuilder.AppendFormat(formatString, programArg.ToRawString());
                argumentLineLogBuilder.AppendFormat(formatString, programArg.ToSecureString());
            }

            context.WriteInfo(
                "Running program '{0}': (work. dir='{1}', args = '{2}', timeout = {3})", programExePath, workingDirectory, argumentLineLogBuilder, executionTimeout);

            lastExitCode = processRunner.Run(programExePath, argumentLineBuilder.ToString(), workingDirectory, executionTimeout, ProcessOutputDataReceived, ProcessErrorDataReceived);

            context.WriteInfo("Exit code: {0}", lastExitCode);

            if (false == ignoreExitCodes && lastExitCode != 0)
                context.Fail("Program '{0}' returned exit code {1}.", programExePath, lastExitCode);
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
        private TimeSpan executionTimeout = TimeSpan.Zero;
        private IProcessRunner processRunner = new ProcessRunner();

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