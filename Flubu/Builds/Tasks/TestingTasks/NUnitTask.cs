using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.TestingTasks
{
    /// <summary>
    /// Run NUnit tests with NUnit console runner.
    /// </summary>
    [SuppressMessage ("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class NUnitTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTask"/> class.
        /// </summary>
        /// <param name="testAssemblyFileName">File name of the assembly containing the test code.</param>
        /// <param name="nunitConsoleFileName">Path to the NUnit-console.exe</param>
        /// <param name="workingDirectory">Working directory to use.</param>
        public NUnitTask (
            string testAssemblyFileName,
            string nunitConsoleFileName,
            string workingDirectory)
        {
            this.nunitConsoleFileName = nunitConsoleFileName;
            TestAssemblyFileName = testAssemblyFileName;
            WorkingDirectory = workingDirectory;
        }

        /// <summary>
        /// Gets or sets unit test working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets assembly to test.
        /// </summary>
        public string TestAssemblyFileName { get; set; }

        /// <summary>
        /// Gets or sets the target .NET framework NUnit console should run under.
        /// </summary>
        public string TargetFramework { get; set; }

        /// <summary>
        /// Gets or sets tests categories that will be excluded from test.
        /// </summary>
        public string ExcludeCategories { get; set; }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return string.Format (
                    CultureInfo.InvariantCulture,
                    "Execute NUnit unit tests. Assembly:{0}",
                    TestAssemblyFileName);
            }
        }

        /// <summary>
        /// Abstract method defining the actual work for a task.
        /// </summary>
        /// <remarks>This method has to be implemented by the inheriting task.</remarks>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            RunProgramTask task = new RunProgramTask (nunitConsoleFileName, false);

            task
                .SetWorkingDir (WorkingDirectory)
                .EncloseParametersInQuotes (true)
                .AddArgument (TestAssemblyFileName)
                .AddArgument ("/nodots")
                .AddArgument ("/labels")
                .AddArgument ("/noshadow");

            if (!string.IsNullOrEmpty (TargetFramework))
                task.AddArgument ("/framework:{0}", TargetFramework);

            if (!string.IsNullOrEmpty (ExcludeCategories))
                task.AddArgument ("/exclude={0}", ExcludeCategories);

            task.Execute (context);
        }

        private readonly string nunitConsoleFileName;
    }
}