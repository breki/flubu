using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Flubu.Services;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.DnxTasks
{
    public class MSBuildTask : TaskBase
    {
        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "MSBuild ({0})", string.Join(",", Parameters)); }
        }

        public Collection<string> Parameters { get; private set; }

        public MSBuildTask WithParams(params string[] parameters)
        {
            Parameters = new Collection<string>(parameters);
            return this;
        }

        public MSBuildTask AddParam(string param)
        {
            Parameters.Add(param);
            return this;
        }

        public MSBuildTask AddParam(string format, params object[] args)
        {
            Parameters.Add(string.Format(CultureInfo.InvariantCulture, format, args));
            return this;
        }

        public string WorkFolder { get; private set; }

        public MSBuildTask WorkingFolder(string folder)
        {
            WorkFolder = folder;
            return this;
        }

        public string SolutionFile { get; private set; }

        public MSBuildTask ForSolution(string fileName)
        {
            SolutionFile = fileName;
            return this;
        }

        public MSBuildTask TasksFactory(ICommonTasksFactory factory)
        {
            CommonTasksFactory = factory;
            return this;
        }

        public ICommonTasksFactory CommonTasksFactory { get; private set; }

        public MSBuildTask EnvironmentService(IFlubuEnvironmentService service)
        {
            FlubuEnvironmentService = service;
            return this;
        }

        public IFlubuEnvironmentService FlubuEnvironmentService { get; private set; }
        
        public static MSBuildTask CreateCompile(string solution, string configuration, bool useSolutionAsWorkingFolder = true, string target = null)
        {
            MSBuildTask task = new MSBuildTask(solution)
                .AddParam("/p:Configuration={0}", configuration)
                .AddParam("/p:Platform=Any CPU")
                .AddParam("/consoleloggerparameters:NoSummary");

            if (useSolutionAsWorkingFolder)
                task.WorkFolder = Path.GetDirectoryName(solution);

            if (target != null)
                task.AddParam("/t:{0}", target);

            return task;
        }

        public static MSBuildTask Create(params string[] parameters)
        {
            return new MSBuildTask()
                .WithParams(parameters);
        }

        public Version ToolsVersion { get; private set; }

        public MSBuildTask WithToolsVersion(Version version)
        {
            ToolsVersion = version;
            return this;
        }

        public int CpuCount { get; private set; }

        public MSBuildTask MaxCpu(int count)
        {
            CpuCount = count;
            return this;
        }

        public string ExecutablePath { get; private set; }

        public MSBuildTask MSBuildPath(string fullFilePath)
        {
            ExecutablePath = fullFilePath;
            return this;
        }

        protected MSBuildTask()
        {
            Parameters = new Collection<string>();
            CommonTasksFactory = new CommonTasksFactory();
            FlubuEnvironmentService = new FlubuEnvironmentService();
            CpuCount = 3;
        }

        protected MSBuildTask(string solutionFile) : this()
        {
            SolutionFile = solutionFile;
        }

        protected override void DoExecute (ITaskContext context)
        {
            string msbuildPath = FindExecutableBuildPath(context);

            IRunProgramTask task = CommonTasksFactory.CreateRunProgramTask(msbuildPath);
            task.EncloseParametersInQuotes(false);

            if (!string.IsNullOrEmpty(SolutionFile))
                task.AddArgument(SolutionFile);

            foreach (var p in Parameters)
            {
                task.AddArgument(p);
            }

            if (CpuCount > 0)
                task.AddArgument("/maxcpucount:{0}", CpuCount);

            task.Execute (context);
        }

        private string FindExecutableBuildPath(ITaskContext context)
        {
            if (!string.IsNullOrEmpty(ExecutablePath))
                return ExecutablePath;
            
            string msbuildPath = context.Properties.Get<string>(BuildProps.MSBuildPath);

            if (string.IsNullOrEmpty(msbuildPath))
                return msbuildPath;

            IDictionary<Version, string> msbuilds = FlubuEnvironmentService.ListAvailableMSBuildToolsVersions();
            if (msbuilds.Count == 0)
                throw new TaskExecutionException ("No MSBuild tools found on the system");

            if (ToolsVersion != null)
            {
                if (!msbuilds.TryGetValue(ToolsVersion, out msbuildPath))
                {
                    KeyValuePair<Version, string> higherVersion = msbuilds.FirstOrDefault(x => x.Key > ToolsVersion);
                    if (higherVersion.Equals(default(KeyValuePair<Version, string>)))
                        throw new TaskExecutionException("Requested MSBuild tools version {0} not found and there are no higher versions".Fmt(ToolsVersion));

                    context.WriteInfo (
                        "Requested MSBuild tools version {0} not found, using a higher version {1}",
                        ToolsVersion, 
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
    }
}