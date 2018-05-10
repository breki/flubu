using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Flubu.Services;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.DnxTasks
{
    public class MSBuildTask : TaskBase, IExternalProcessTask<MSBuildTask>
    {
        public override string Description =>
            string.Format(
                CultureInfo.InvariantCulture,
                "MSBuild ({0})",
                string.Join(",", Parameters));

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

        public TimeSpan? Timeout { get; private set; }

        public string SolutionFile { get; private set; }

        public MSBuildTask ForSolution(string fileName)
        {
            SolutionFile = fileName;
            return this;
        }

        public MSBuildTask ExecutionTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public MSBuildTask TasksFactory(ICommonTasksFactory factory)
        {
            CommonTasksFactory = factory;
            return this;
        }

        public ICommonTasksFactory CommonTasksFactory { get; private set; }

        public static MSBuildTask CreateCompile(
            string solution,
            string configuration,
            bool useSolutionAsWorkingFolder = true,
            string target = null)
        {
            MSBuildTask task = new MSBuildTask(solution)
                .AddParam("/p:Configuration={0}", configuration)
                .AddParam("/p:Platform=\"Any CPU\"")
                .AddParam("/consoleloggerparameters:NoSummary");

            if (useSolutionAsWorkingFolder)
                task.WorkFolder = Path.GetDirectoryName(solution);

            if (target != null)
                task.AddParam("/t:{0}", target);

            return task;
        }

        public static MSBuildTask CreateRaw(params string[] parameters)
        {
            return new MSBuildTask()
                .WithParams(parameters);
        }

        public static MSBuildTask Create(string solutionFile)
        {
            return new MSBuildTask(solutionFile);
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

            if (!string.IsNullOrEmpty(WorkFolder))
                task.SetWorkingDir(WorkFolder);

            if (!string.IsNullOrEmpty(SolutionFile))
                task.AddArgument(SolutionFile);

            if (Timeout != null)
                task.ExecutionTimeout(Timeout.Value);

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
            
            return context.Properties.Get<string>(BuildProps.MSBuildPath);
        }
    }
}