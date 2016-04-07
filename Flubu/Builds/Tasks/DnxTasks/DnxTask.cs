using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Flubu.Services;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.DnxTasks
{
    public class DnxTask : TaskBase
    {
        public DnxTask()
        {
            ClrVersionName = DnuTask.LatestClrVersion;
            CommonTasksFactory = new CommonTasksFactory();
        }

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Dnx {0}: ({1})", ClrVersionName, string.Join(",", Parameters)); }
        }

        public Collection<string> Parameters { get; private set; }
        public string ClrVersionName { get; private set; }
        public string WorkFolder { get; private set; }

        public DnxTask WithParams(params string[] parameters)
        {
            Parameters = new Collection<string>(parameters);
            return this;
        }

        public DnxTask AddParam(string param)
        {
            Parameters.Add(param);
            return this;
        }

        public DnxTask AddParam(string format, params object[] args)
        {
            Parameters.Add(string.Format(CultureInfo.InvariantCulture, format, args));
            return this;
        }

        public DnxTask WithClrVersion(string version)
        {
            ClrVersionName = version;
            return this;
        }

        public DnxTask WorkingFolder(string folder)
        {
            WorkFolder = folder;
            return this;
        }

        public DnxTask TasksFactory(ICommonTasksFactory factory)
        {
            CommonTasksFactory = factory;
            return this;
        }

        public ICommonTasksFactory CommonTasksFactory { get; private set; }

        public static DnxTask Create()
        {
            return new DnxTask();
        }

        protected override void DoExecute(ITaskContext context)
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string dnx = Path.Combine(root, string.Format(CultureInfo.InvariantCulture, @".dnx\runtimes\{0}\bin", ClrVersionName));

            IRunProgramTask t = CommonTasksFactory.CreateRunProgramTask(Path.Combine(dnx, "dnx.exe"));

            if (!string.IsNullOrEmpty(WorkFolder))
                t.SetWorkingDir(WorkFolder);

            t.EncloseParametersInQuotes(false);

            foreach (string s in Parameters)
            {
                t.AddArgument(s);
            }

            t.Execute(context);
        }
    }
}
