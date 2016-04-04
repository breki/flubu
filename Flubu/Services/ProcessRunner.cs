using System;
using System.Diagnostics;
using System.IO;

namespace Flubu.Services
{
    public class ProcessRunner : IProcessRunner
    {
        public int Run(
            string programExePath,
            string arguments,
            string workingDirectory,
            TimeSpan? executionTimeout,
            DataReceivedEventHandler outputDataReceived,
            DataReceivedEventHandler errorDataReceived)
        {
            using (Process process = new Process ())
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo (programExePath, arguments)
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(programExePath) ?? string.Empty
                };

                process.StartInfo = processStartInfo;

                if (errorDataReceived != null)
                    process.ErrorDataReceived += errorDataReceived;

                if (outputDataReceived != null)
                    process.OutputDataReceived += outputDataReceived;

                process.Start ();

                process.BeginOutputReadLine ();
                process.BeginErrorReadLine ();

                if (executionTimeout == null || executionTimeout.Value.TotalMilliseconds <= 0)
                    process.WaitForExit ();
                else
                    process.WaitForExit (executionTimeout.Value.Milliseconds);

                if (errorDataReceived != null)
                    process.ErrorDataReceived -= errorDataReceived;

                if (outputDataReceived != null)
                    process.OutputDataReceived -= outputDataReceived;

                return process.ExitCode;
            }
        }
    }
}