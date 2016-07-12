using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string msbuildPath;

            IDictionary<Version, string> msbuilds = flubuEnvironmentService.ListAvailableMSBuildToolsVersions();
            if (msbuilds.Count == 0)
                throw new TaskExecutionException ("No MSBuild tools found on the system");

            if (toolsVersion != null)
            {
                if (!msbuilds.TryGetValue(toolsVersion, out msbuildPath))
                {
                    KeyValuePair<Version, string> higherVersion = msbuilds.FirstOrDefault(x => x.Key > toolsVersion);
                    if (higherVersion.Equals(default(KeyValuePair<Version, string>)))
                        throw new TaskExecutionException("Requested MSBuild tools version {0} not found and there are no higher versions".Fmt(toolsVersion));

                    context.WriteInfo (
                        "Requested MSBuild tools version {0} not found, using a higher version {1}", 
                        toolsVersion, 
                        higherVersion.Key);
                    msbuildPath = higherVersion.Value;
                }
            }
            else
            {
                KeyValuePair<Version, string> highestVersion = msbuilds.Last();
                context.WriteInfo (
                    "Since MSBuild tools version was not explicity specified, using the highest MSBuild tools version found ({0})", 
                    highestVersion.Key);
                msbuildPath = highestVersion.Value;
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