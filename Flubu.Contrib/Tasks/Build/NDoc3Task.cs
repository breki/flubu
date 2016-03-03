namespace Flubu.Tasks.Build
{
    using System.Collections.ObjectModel;
    using System.Globalization;

    using Flubu;
    using Flubu.Tasks.Processes;

    public class NDoc3Task : TaskBase
    {
        public NDoc3Task(Collection<string> assembliesToDocument)
        {
            NDoc3Path = "lib\\NDoc3\\NDoc3Console.exe";
            WorkingDirectory = ".";
            AssembliesToDocument = assembliesToDocument;
        }

        public NDoc3Task(params string[] arguments)
        {
            WorkingDirectory = ".";
            NDoc3Path = "lib\\NDoc3\\NDoc3Console.exe";
            AssembliesToDocument = new Collection<string>(arguments);
        }

        /// <summary>
        ///   Gets or sets documentation working directory. (default=.)
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        ///   Gets or sets documentation output directory. (default=.\doc)
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        ///   Gets assemblies to document.
        /// </summary>
        public Collection<string> AssembliesToDocument { get; private set; }

        /// <summary>
        ///   Gets or sets NDoc3 application path.
        /// </summary>
        public string NDoc3Path { get; set; }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Execute NDoc task.");
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            IRunProgramTask task = new RunProgramTask(NDoc3Path)
                .SetWorkingDir(WorkingDirectory);

            foreach (string assembly in AssembliesToDocument)
            {
                task.AddArgument("\"{0}\" ", assembly);
            }

            task.AddArgument("\"{0}\" ", "-OutputTarget=Web");

            if (!string.IsNullOrEmpty(OutputDirectory))
                task.AddArgument("\"-OutputDirectory={0}\" ", OutputDirectory);

            task.Execute(context);
        }
    }
}