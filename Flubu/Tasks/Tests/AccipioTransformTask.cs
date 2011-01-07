using System;
using System.Globalization;
using System.IO;
using System.Text;
using Flubu.Tasks.Processes;

namespace Flubu.Tasks.Tests
{
    public class AccipioTransformTask : TaskBase
    {
        public static void Execute(ITaskContext environment, string workingFolder)
        {
            AccipioTransformTask task = new AccipioTransformTask(workingFolder);
            task.Execute(environment);
        }

        public AccipioTransformTask(string workingDir)
        {
            WorkingDirectory = workingDir;
            AccipioPath = @"lib\Accipio.Console\Accipio.Console.exe";
            AccipioTransformsPath = "AccipioTransforms";
            GallioReportPath = ".\\Reports\\LastTestResults.xml";
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Execute accipio transform. Gallio reports:{0}, Output:{1}",
                    GallioReportPath,
                    AccipioTransformsPath);
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
        /// Gets or sets Accipio output path. (WorkingDir\GallioTestReports\LastTestResults.xml)
        /// </summary>
        public string GallioReportPath { get; set; }

        protected override void DoExecute(ITaskContext context)
        {
            StringBuilder argumentLineBuilder = new StringBuilder();
            argumentLineBuilder.Append("transform ");
            argumentLineBuilder.AppendFormat("\"-i={0}\" ", GallioReportPath);
            argumentLineBuilder.AppendFormat("\"-o={0}\" ", AccipioTransformsPath);

            RunProgramTask task = new RunProgramTask(AccipioPath, argumentLineBuilder.ToString(), new TimeSpan(0, 1, 0, 0))
            {
                WorkingDirectory = WorkingDirectory,
            };
            task.Execute(context);
        }
    }
}
