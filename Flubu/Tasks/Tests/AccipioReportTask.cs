using System;
using System.Globalization;
using System.IO;
using System.Text;
using Flubu.Tasks.Processes;

namespace Flubu.Tasks.Tests
{
    public class AccipioReportTask : TaskBase
    {
        public static void Execute(ITaskContext environment, string workingFolder)
        {
            AccipioReportTask task = new AccipioReportTask(workingFolder);
            task.Execute(environment);
        }

        public AccipioReportTask(string workingDir)
        {
            WorkingDirectory = workingDir;
            AccipioPath = @"lib\Accipio.Console\Accipio.Console.exe";
            AccipioTransformsPath = "AccipioTransforms";
            AccipioOutputPath = "AccipioTestReports";
            AccipioTemplatePath = "Templates\\TestReports";
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Execute accipio report. Accipiot transforms:{0}, Output:{1}",
                    AccipioTransformsPath,
                    AccipioOutputPath);
            }
        }

        /// <summary>
        /// Gets or sets unit test working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets Accipio application path. (lib\Accipio.Console\Accipio.Console.exe)
        /// </summary>
        public string AccipioPath { get; set; }

        /// <summary>
        /// Gets or sets Accipio transforms path. (WorkingDir\AccipioTransforms)
        /// </summary>
        public string AccipioTransformsPath { get; set; }
        
        /// <summary>
        /// Gets or sets Accipio output path. (WorkingDir\AccipioTestReports)
        /// </summary>
        public string AccipioOutputPath { get; set; }

        /// <summary>
        /// Gets or sets Accipio template path. (AccipioPath\TestReports)
        /// </summary>
        public string AccipioTemplatePath { get; set; }

        protected override void DoExecute(ITaskContext context)
        {
            StringBuilder argumentLineBuilder = new StringBuilder();
            argumentLineBuilder.Append("report ");
            argumentLineBuilder.AppendFormat("\"-i={0}\" ", AccipioTransformsPath);
            argumentLineBuilder.AppendFormat("\"-o={0}\" ", AccipioOutputPath);
            argumentLineBuilder.Append("\"-p=Push Dispatcher\" ");
            argumentLineBuilder.AppendFormat("\"-t={0}\" ", AccipioTemplatePath);
            argumentLineBuilder.AppendFormat("\"-c={0}\" ", "TestReport.css");

            RunProgramTask task = new RunProgramTask(AccipioPath, argumentLineBuilder.ToString(), new TimeSpan(0, 1, 0, 0))
            {
                WorkingDirectory = WorkingDirectory,
            };
            task.Execute(context);
        }
    }
}
