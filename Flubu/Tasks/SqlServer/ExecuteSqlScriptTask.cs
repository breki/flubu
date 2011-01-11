using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Flubu.Tasks.SqlServer
{
    public class ExecuteSqlScriptTask : TaskBase
    {
        public override string Description
        {
            get
            {
                if (string.IsNullOrEmpty(CommandText))
                {
                    return string.Format(CultureInfo.InvariantCulture, "Execute SQL script '{0}'", scriptFilePath);
                }

                return string.Format(CultureInfo.InvariantCulture, "Execute SQL command '{0}'", CommandText);
            }
        }

        public static void ExecuteSqlScriptFile(
            ITaskContext context,
            string connectionString, 
            string scriptFilePath)
        {
            ExecuteSqlScriptTask task = new ExecuteSqlScriptTask(connectionString, scriptFilePath, null);
            task.Execute(context);
        }

        public static void ExecuteSqlCommand(
            ITaskContext context,
            string connectionString, 
            string commandText)
        {
            ExecuteSqlScriptTask task = new ExecuteSqlScriptTask(connectionString, null, commandText);
            task.Execute(context);
        }

        protected ExecuteSqlScriptTask(string connectionString, string scriptFilePath, string commandText)
        {
            this.connectionString = connectionString;
            this.scriptFilePath = scriptFilePath;
            CommandText = commandText;
            if (string.IsNullOrEmpty(commandText)) return;

            string file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using (StreamWriter writer = File.CreateText(file))
            {
                writer.WriteLine(commandText);
            }

            this.scriptFilePath = file;
            deleteTempScript = true;
        }

        protected override void DoExecute(ITaskContext context)
        {
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(connectionString);

            if (!File.Exists(scriptFilePath))
                throw new FileNotFoundException("The file does not exist.", scriptFilePath);

            // create cmd line
            StringBuilder cmd =
                new StringBuilder(string.Format(CultureInfo.InvariantCulture, "OSQL -S \"{0}\" -i \"{1}\" -n", connStringBuilder.DataSource, scriptFilePath));

            if (connStringBuilder.IntegratedSecurity)
                cmd.Append(" -E");
            else
                cmd.AppendFormat(CultureInfo.InvariantCulture, " -U {0} -P {1}", connStringBuilder.UserID, connStringBuilder.Password);

            if (!string.IsNullOrEmpty(connStringBuilder.InitialCatalog))
            {
                cmd.AppendFormat(CultureInfo.InvariantCulture, " -d \"{0}\"", connStringBuilder.InitialCatalog);
            }
            
            Env = context;

            // create the process
            Process process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = "cmd",
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                }
            };
            process.ErrorDataReceived += ProcessErrorDataReceived;
            process.OutputDataReceived += ProcessOutputDataReceived;

            // start the application
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.StandardInput.WriteLine("@ECHO OFF");
            process.StandardInput.WriteLine(cmd.ToString());
            process.StandardInput.WriteLine("EXIT");
            process.StandardInput.Flush();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new TaskExecutionException(string.Format(CultureInfo.InvariantCulture, "OSQL exited with code {0}.", process.ExitCode));

            //Reading output after waitforexit creates deadlock as available output buffer is fully filled
            //output.Write(process.StandardOutput.ReadToEnd());
            if (deleteTempScript)
                File.Delete(scriptFilePath);
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                Env.WriteError(e.Data);
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                Env.WriteError(e.Data);
        }

        private ITaskContext Env { get; set; }

        private string CommandText { get; set; }

        private readonly string connectionString;
        private readonly string scriptFilePath;
        private readonly bool deleteTempScript;
    }
}