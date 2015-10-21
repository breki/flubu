using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.TestingTasks
{
    /// <summary>
    /// Run tests with NUnit.
    /// <example>
    /// NUnitTask task = new NUnitTask(Path.Combine("Testing\\UnitTests\\bin", BuildConfiguration),
    ///            "Hsl.PD.UnitTests.dll")
    ///        {
    ///            ExcludeCategories = "Cassini, LongTest",
    ///            NUnitPath = MakePathFromRootDir(@"lib\NUnit\bin\net-2.0\nunit-console-x86.exe"),
    ///        };
    ///        task.Execute(ScriptExecutionEnvironment);
    /// </example>
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class NUnitTask : TaskBase
    {
        public static void Execute(ITaskContext environment, string workingFolder, string assemblyToTest)
        {
            NUnitTask task = new NUnitTask(workingFolder, assemblyToTest);
            task.Execute(environment);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTask"/> class.
        /// </summary>
        /// <param name="workingDirectory">Working directory to use.</param>
        /// <param name="assemblyToTest">Assembly to test.</param>
        public NUnitTask(string workingDirectory, string assemblyToTest)
        {
            NUnitPath = @"lib\NUnit\bin\net-2.0\nunit-console-x86.exe";
            AssemblyToTest = assemblyToTest;
            WorkingDirectory = workingDirectory;
        }

        /// <summary>
        /// Gets or sets unit test working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }
        
        /// <summary>
        /// Gets or sets assembly to test.
        /// </summary>
        public string AssemblyToTest { get; set; }

        /// <summary>
        /// Gets or sets NUnit application path.
        /// </summary>
        public string NUnitPath { get; set; }
        
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return string.Format(
                CultureInfo.InvariantCulture,
                "Execute NUnit unit tests. Assembly:{0}",
                AssemblyToTest);
            }
        }

        /// <summary>
        /// Gets or sets tests categories that will be excluded from test.
        /// </summary>
        public string ExcludeCategories { get; set; }

        /// <summary>
        /// Abstract method defining the actual work for a task.
        /// </summary>
        /// <remarks>This method has to be implemented by the inheriting task.</remarks>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute(ITaskContext context)
        {
            RunProgramTask task = new RunProgramTask(NUnitPath, false);

            task
                .SetWorkingDir(WorkingDirectory)
                .EncloseParametersInQuotes(true)
                .AddArgument(AssemblyToTest)
                .AddArgument("/nodots")
                .AddArgument("/labels")
                .AddArgument("/noshadow");

            if (!string.IsNullOrEmpty(ExcludeCategories))
                task.AddArgument("/exclude={0}", ExcludeCategories);

            task.Execute(context);
        }
    }
}