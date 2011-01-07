using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Flubu
{
    public class ExternalProgramRunner
    {
        /// <summary>
        /// Gets the exit code of the last external program that was run by the runner.
        /// </summary>
        /// <value>The exit code of the last external program.</value>
        public int LastExitCode
        {
            get { return lastExitCode; }
        }

        public ExternalProgramRunner AddArgument(string argument)
        {
            programArgs.Add(argument);
            return this;
        }

        public ExternalProgramRunner AddArgument(string format, params object[] args)
        {
            programArgs.Add(string.Format(CultureInfo.InvariantCulture, format, args));
            return this;
        }

        public ExternalProgramRunner Run(string programExePath)
        {
            return Run(programExePath, false);
        }

        public ExternalProgramRunner Run(string programExePath, bool ignoreExitCodes)
        {
            try
            {
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

                    runner.Log(
                        "Running program '{0}' (work. dir='{1}', args = '{2}')",
                        programExePath,
                        processStartInfo.WorkingDirectory,
                        argumentLineBuilder);

                    process.StartInfo = processStartInfo;
                    process.ErrorDataReceived += new DataReceivedEventHandler(Process_ErrorDataReceived);
                    process.OutputDataReceived += new DataReceivedEventHandler(Process_OutputDataReceived);
                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    runner.Log("Exit code: {0}", process.ExitCode);

                    lastExitCode = process.ExitCode;

                    if (false == ignoreExitCodes && process.ExitCode != 0)
                        runner.Fail("Program '{0}' returned exit code {1}.", programExePath, process.ExitCode);
                }
            }
            finally
            {
                programArgs.Clear();
            }

            return this;
        }

        public ExternalProgramRunner SetWorkingDir (string workingDirectory)
        {
            this.workingDirectory = workingDirectory;
            return this;
        }

        public ExternalProgramRunner UseProgramDirAsWorkingDir()
        {
            workingDirectory = null;
            return this;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            runner.TaskContext.LogError(e.Data);
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            runner.TaskContext.LogMessage(e.Data);
        }

        private int lastExitCode;
        private List<string> programArgs = new List<string>();
        private string workingDirectory = ".";
    }
}