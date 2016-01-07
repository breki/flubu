using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.SolutionTasks
{
    public class CompileSolutionTask : TaskBase
    {
        public CompileSolutionTask (
            string solutionFileName, 
            string buildConfiguration, 
            string dotNetVersion)
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

        public string Target
        {
            get { return target; }
            set { target = value; }
        }

        public string ToolsVersion
        {
            get { return toolsVersion; }
            set { toolsVersion = value; }
        }

        public bool UseSolutionDirAsWorkingDir
        {
            get { return useSolutionDirAsWorkingDir; }
            set { useSolutionDirAsWorkingDir = value; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            string msbuildPath;

            bool isDifferentToolsVersion = !string.IsNullOrEmpty(toolsVersion) && !dotNetVersion.Equals(toolsVersion);

            if (isDifferentToolsVersion)
                msbuildPath = FlubuEnvironment.GetDotNetFWDir (toolsVersion);
            else
                msbuildPath = FlubuEnvironment.GetDotNetFWDir (dotNetVersion);

            RunProgramTask task = new RunProgramTask (Path.Combine (msbuildPath, @"msbuild.exe"), false);
            task
                .AddArgument (solutionFileName)
                .AddArgument ("/p:Configuration={0}", buildConfiguration)
                .AddArgument ("/p:Platform=Any CPU")
                .AddArgument ("/consoleloggerparameters:NoSummary")
                .AddArgument ("/maxcpucount:{0}", maxCpuCount);

            if (isDifferentToolsVersion)
                task.AddArgument("/toolsVersion:{0}", toolsVersion.Substring(1));
 
            if (useSolutionDirAsWorkingDir)
                task.SetWorkingDir(Path.GetDirectoryName(solutionFileName));

            if (target != null)
                task.AddArgument ("/t:{0}", target);

            task.Execute (context);
        }

        private string target;
        private readonly string solutionFileName;
        private readonly string buildConfiguration;
        private readonly string dotNetVersion;
        private string toolsVersion;
        private bool useSolutionDirAsWorkingDir;
        private int maxCpuCount = 3;
    }
}