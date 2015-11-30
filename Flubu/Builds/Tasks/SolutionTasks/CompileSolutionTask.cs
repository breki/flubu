using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.SolutionTasks
{
    public class CompileSolutionTask : TaskBase
    {
        public CompileSolutionTask(string solutionFileName, string buildConfiguration, string dotNetVersion)
        {
            this.solutionFileName = solutionFileName;
            this.buildConfiguration = buildConfiguration;
            this.dotNetVersion = dotNetVersion;
        }

        public override string Description
        {
            get { return "Compile VS solution"; }
        }

        public int MaxCpuCount
        {
            get { return maxCpuCount; }
            set { maxCpuCount = value; }
        }

        protected override void DoExecute(ITaskContext context)
        {
            string msbuildPath = FlubuEnvironment.GetDotNetFWDir(dotNetVersion);

            RunProgramTask task = new RunProgramTask(Path.Combine(msbuildPath, @"msbuild.exe"), false);
            task
                .AddArgument(solutionFileName)
                .AddArgument("/p:Configuration={0}", buildConfiguration)
                .AddArgument("/p:Platform=Any CPU")
                .AddArgument("/consoleloggerparameters:NoSummary")
                .AddArgument("/maxcpucount:{0}", maxCpuCount)
                .Execute(context);
        }

        private readonly string solutionFileName;
        private readonly string buildConfiguration;
        private readonly string dotNetVersion;
        private int maxCpuCount = 3;
    }
}