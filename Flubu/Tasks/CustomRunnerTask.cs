using System;
using System.Globalization;
using Flubu;

namespace Flubu.Tasks
{
    public class CustomRunnerTask<TRunner> : TaskBase
        where TRunner : FlubuRunner<TRunner>
    {
        public CustomRunnerTask(
            TRunner runner,
            Action<TRunner> taskAction,
            string taskDescriptionFormat,
            params object[] taskDescriptionArgs)
        {
            this.runner = runner;
            this.taskAction = taskAction;
            this.taskDescriptionFormat = taskDescriptionFormat;
            this.taskDescriptionArgs = taskDescriptionArgs;
        }

        public override string TaskDescription
        {
            get { return String.Format(CultureInfo.InvariantCulture, taskDescriptionFormat, taskDescriptionArgs); }
        }

        protected override void DoExecute(ITaskContext context)
        {
            taskAction(runner);
        }

        private readonly TRunner runner;
        private readonly Action<TRunner> taskAction;
        private readonly object[] taskDescriptionArgs;
        private readonly string taskDescriptionFormat;
    }
}