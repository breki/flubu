using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Flubu.Tasks.Processes
{
    public class RunProgramTask : TaskBase
    {
        public RunProgramTask(string programExePath, bool ignoreExitCodes)
        {
            this.programExePath = programExePath;
            this.ignoreExitCodes = ignoreExitCodes;
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

        public RunProgramTask SetWorkingDir(string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
            return this;
        }

        public RunProgramTask UseProgramDirAsWorkingDir()
        {
            workingDirectory = null;
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            this.context = context;

            using (Process process = new Process())
            {
                StringBuilder argumentLineBuilder = new StringBuilder();
                foreach (string programArg in programArgs)
                    argumentLineBuilder.AppendFormat("\"{0}\" ", programArg);

                ProcessStartInfo processStartInfo = new ProcessStartInfo(programExePath, argumentLineBuilder.ToString());
                processStartInfo.CreateNoWindow = true;
                processStartInfo.ErrorDialog = false;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;

                if (workingDirectory == null)
                    processStartInfo.WorkingDirectory = Path.GetDirectoryName(programExePath);
                else
                    processStartInfo.WorkingDirectory = workingDirectory;

                context.WriteInfo(
                    "Running program '{0}' (work. dir='{1}', args = '{2}')",
                    programExePath,
                    processStartInfo.WorkingDirectory,
                    argumentLineBuilder);

                process.StartInfo = processStartInfo;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                context.WriteInfo("Exit code: {0}", process.ExitCode);

                lastExitCode = process.ExitCode;

                if (false == ignoreExitCodes && process.ExitCode != 0)
                    context.Fail("Program '{0}' returned exit code {1}.", programExePath, process.ExitCode);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            context.WriteError(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            context.WriteInfo(e.Data);
        }

        private ITaskContext context;
        private readonly bool ignoreExitCodes;
        private int lastExitCode;
        private readonly string programExePath;
        private List<string> programArgs = new List<string>();
        private string workingDirectory = ".";
    }
}
