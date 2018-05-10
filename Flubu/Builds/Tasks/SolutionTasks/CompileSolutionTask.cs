using System.IO;
using Flubu.Services;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.SolutionTasks
{
    public class CompileSolutionTask : TaskBase
    {
        public CompileSolutionTask (
            string solutionFileName, 
            string buildConfiguration)
        {
            this.solutionFileName = solutionFileName;
            this.buildConfiguration = buildConfiguration;
        }

        public override string Description => "Compile VS solution";

        public int MaxCpuCount { get; set; } = 3;

        public string Target { get; set; }

        public bool UseSolutionDirAsWorkingDir { get; set; }

        public ICommonTasksFactory CommonTasksFactory { get; set; } = new CommonTasksFactory();

        protected override void DoExecute (ITaskContext context)
        {
            string msbuildPath = FindMSBuildPath(context);

            IRunProgramTask task = CommonTasksFactory.CreateRunProgramTask(msbuildPath);
            task
                .AddArgument (solutionFileName)
                .AddArgument ("/p:Configuration={0}", buildConfiguration)
                .AddArgument ("/p:Platform=Any CPU")
                .AddArgument ("/consoleloggerparameters:NoSummary")
                .AddArgument ("/maxcpucount:{0}", MaxCpuCount);

            if (UseSolutionDirAsWorkingDir)
                task.SetWorkingDir(Path.GetDirectoryName(solutionFileName));

            if (Target != null)
                task.AddArgument ("/t:{0}", Target);

            task.Execute (context);
        }

        private static string FindMSBuildPath(ITaskContext context)
        {
            return context.Properties.Get<string>(BuildProps.MSBuildPath);
        }

        private readonly string solutionFileName;
        private readonly string buildConfiguration;
    }
}