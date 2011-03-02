using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Tasks.FileSystem;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks
{
    public class RunGallioTestsTask : TaskBase
    {
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "4#")]
        public RunGallioTestsTask(
            string projectName, 
            VSSolution solution,
            string buildConfiguration,
            string gallioEchoExePath,
            ref int testRunCounter,
            string buildLogsDirectory)
        {
            this.projectName = projectName;
            this.solution = solution;
            this.buildConfiguration = buildConfiguration;
            this.gallioEchoExePath = gallioEchoExePath;
            this.testRunCounter = testRunCounter;
            this.buildLogsDirectory = buildLogsDirectory;
        }

        public string Filter
        {
            get { return filter; }
            set { filter = value; }
        }

        public override string Description
        {
            get
            {
                string text = string.Format(
                    CultureInfo.InvariantCulture,
                    "Run Gallio tests on '{0}' project using filter '{1}'",
                    projectName,
                    filter);
                return text;
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            EnsureBuildLogsTestDirectoryExists(context);

            VSProjectWithFileInfo project = 
                (VSProjectWithFileInfo)solution.FindProjectByName(projectName);
            FileFullPath projectTarget = project.ProjectDirectoryPath.CombineWith(project.GetProjectOutputPath(buildConfiguration))
                .AddFileName("{0}.dll", project.ProjectName);

            RunProgramTask gallioTask = new RunProgramTask(gallioEchoExePath);
            gallioTask
                .AddArgument(projectTarget.ToString())
                .AddArgument("/report-directory:{0}", buildLogsDirectory)
                .AddArgument("/report-name-format:TestResults-{0}", testRunCounter)
                .AddArgument("/report-type:xml")
                .AddArgument("/verbosity:verbose");

            if (false == String.IsNullOrEmpty(filter))
                gallioTask.AddArgument("/filter:{0}", filter);

            if (context.Properties.Has(BuildProps.TargetDotNetVersion))
                gallioTask.AddArgument("/rv:{0}", context.Properties.Get<string>(BuildProps.TargetDotNetVersion));

            gallioTask.Execute(context);

            testRunCounter++;
        }

        protected void EnsureBuildLogsTestDirectoryExists(ITaskContext context)
        {
            CreateDirectoryTask task = new CreateDirectoryTask(buildLogsDirectory, false);
            task.Execute(context);
        }

        private readonly string projectName;
        private readonly VSSolution solution;
        private readonly string buildConfiguration;
        private readonly string gallioEchoExePath;
        private int testRunCounter;
        private readonly string buildLogsDirectory;
        private string filter;
    }
}