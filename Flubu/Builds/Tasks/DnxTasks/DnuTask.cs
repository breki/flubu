using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Flubu.Services;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.DnxTasks
{
    public class DnuTask : TaskBase, IExternalProcessTask<DnuTask>
    {
        public DnuTask()
        {
            Parameters = new Collection<string>();
            ClrVersionName = LatestClrVersion;
            CommonTasksFactory = new CommonTasksFactory();
        }

        public const string LatestClrVersion = "dnx-clr-win-x64.1.0.0-rc1-final";

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Dnu {0}: ({1})", ClrVersionName, string.Join(",", Parameters)); }
        }

        public Collection<string> Parameters { get; private set; }

        public DnuTask WithParams(params string[] parameters)
        {
            Parameters = new Collection<string>(parameters);
            return this;
        }

        public DnuTask AddParam(string param)
        {
            Parameters.Add(param);
            return this;
        }

        public DnuTask AddParam(string format, params object[] args)
        {
            Parameters.Add(string.Format(CultureInfo.InvariantCulture, format, args));
            return this;
        }

        public string ClrVersionName { get; private set; }

        public DnuTask WithClrVersion(string version)
        {
            ClrVersionName = version;
            return this;
        }

        public string WorkFolder { get; private set; }

        public DnuTask WorkingFolder(string folder)
        {
            WorkFolder = folder;
            return this;
        }

        public TimeSpan? Timeout { get; private set; }
        public DnuTask ExecutionTimeout(TimeSpan timeout)
        {
            Timeout = timeout;
            return this;
        }

        public DnuTask TasksFactory(ICommonTasksFactory factory)
        {
            CommonTasksFactory = factory;
            return this;
        }

        public static DnuTask Create()
        {
            return new DnuTask();
        }

        public ICommonTasksFactory CommonTasksFactory { get; private set; }
        protected override void DoExecute(ITaskContext context)
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dnx = Path.Combine(root, String.Format(CultureInfo.InvariantCulture, @".dnx\runtimes\{0}\bin", ClrVersionName));

            IRunProgramTask t = CommonTasksFactory.CreateRunProgramTask(Path.Combine(dnx, "dnu.cmd"));
            t.EncloseParametersInQuotes(false);

            if (Timeout != null)
                t.ExecutionTimeout(Timeout.Value);

            if (!string.IsNullOrEmpty(WorkFolder))
                t.SetWorkingDir(WorkFolder);

            foreach (string s in Parameters)
            {
                t.AddArgument(s);
            }

            t.Execute(context);
        }
    }
}
