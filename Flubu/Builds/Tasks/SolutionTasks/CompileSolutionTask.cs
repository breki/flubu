using System;
using System.Collections.Generic;
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

        public Version ToolsVersion
        {
            get { return toolsVersion; }
            set { toolsVersion = value; }
        }

        public bool UseSolutionDirAsWorkingDir
        {
            get { return useSolutionDirAsWorkingDir; }
            set { useSolutionDirAsWorkingDir = value; }
        }

        public ICommonTasksFactory CommonTasksFactory
        {
            get { return commonTasksFactory; }
            set { commonTasksFactory = value; }
        }

        public IFlubuEnvironmentService FlubuEnvironmentService
        {
            get { return flubuEnvironmentService; }
            set { flubuEnvironmentService = value; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            string msbuildPath = FindMSBuildPath(context);

            // todo next
            IRunProgramTask task = commonTasksFactory.CreateRunProgramTask(msbuildPath);
            task
                .AddArgument (solutionFileName)
                .AddArgument ("/p:Configuration={0}", buildConfiguration)
                .AddArgument ("/p:Platform=Any CPU")
                .AddArgument ("/consoleloggerparameters:NoSummary")
                .AddArgument ("/maxcpucount:{0}", maxCpuCount);

            if (useSolutionDirAsWorkingDir)
                task.SetWorkingDir(Path.GetDirectoryName(solutionFileName));

            if (target != null)
                task.AddArgument ("/t:{0}", target);

            task.Execute (context);
        }

        private string FindMSBuildPath(ITaskContext context)
        {
            string msbuildPath = null;

            IDictionary<Version, string> msbuilds = flubuEnvironmentService.ListAvailableMSBuildToolsVersions();

            if (toolsVersion != null)
            {
                if (!msbuilds.TryGetValue(toolsVersion, out msbuildPath))
                    throw new NotImplementedException("todo next:");
            }

            return Path.Combine(msbuildPath, "MSBuild.exe");
        }

        private string target;
        private readonly string solutionFileName;
        private readonly string buildConfiguration;
        private Version toolsVersion;
        private bool useSolutionDirAsWorkingDir;
        private int maxCpuCount = 3;
        private ICommonTasksFactory commonTasksFactory = new CommonTasksFactory();
        private IFlubuEnvironmentService flubuEnvironmentService = new FlubuEnvironmentService();
    }
}