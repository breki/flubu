using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using Flubu.Builds.VSSolutionBrowsing;
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
            this.testAssemblyFileName = testAssemblyFileName;
            this.workingDirectory = workingDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTask"/> class.
        /// </summary>
        /// <param name="nunitConsoleFileName">full file path to nunit console</param>
        /// <param name="projectName">Unit test project name.</param>
        public NUnitTask(string nunitConsoleFileName, string projectName)
        {
            this.nunitConsoleFileName = nunitConsoleFileName;
            this.projectName = projectName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTask"/> class.
        /// </summary>
        /// <param name="projectName">Unit test project name.</param>
        public NUnitTask(string projectName)
        {
            this.projectName = projectName;
        }

        /// <summary>
        /// Initializes NunitTask with default command line options for nunit V2.
        /// </summary>
        /// <param name="projectName">Unit test project name.</param>
        /// <returns>New instance of nunit task</returns>
        public static NUnitTask ForNunitV2(string projectName)
        {
            var task = new NUnitTask(projectName);
            task.AddNunitCommandLineOption("/nodots")
                .AddNunitCommandLineOption("/labels")
                .AddNunitCommandLineOption("/noshadow");

            return task;
        }

        /// <summary>
        /// Initializes NunitTask with default command line options for nunit V3.
        /// </summary>
        /// <param name="projectName">Unit test project name.</param>
        /// <returns>New instance of nunit task</returns>
        public static NUnitTask ForNunitV3(string projectName)
        {
            var task = new NUnitTask(projectName);
            task.AddNunitCommandLineOption("/labels=All")
                .AddNunitCommandLineOption("/trace=Verbose")
                .AddNunitCommandLineOption("/verbose");

            return task;
        }

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
                    testAssemblyFileName);
            }
        }

        /// <summary>
        /// Excludes category from test. Can be ussed multiple times. Supported only in nunit v3 and above. For v2 use <see cref="AddNunitCommandLineOption"/>
        /// </summary>
        /// <param name="category">The Categorie to be excluded</param>
        /// <returns>The NunitTask</returns>
        public NUnitTask ExcludeCategory(string category)
        {
            if (string.IsNullOrEmpty(categories))
            {
                categories = string.Format(CultureInfo.InvariantCulture, "cat != {0}", category);
            }
            else
            {
                categories = string.Format(CultureInfo.InvariantCulture, "{0} && cat != {1}", categories, category);
            }
          
            return this;
        }

        /// <summary>
        /// Include category in test. Can be ussed multiple times. Supported only in nunit v3 and above. For v2 use <see cref="AddNunitCommandLineOption"/>
        /// </summary>
        /// <param name="category">The category to be included</param>
        /// <returns>The NunitTask</returns>
        public NUnitTask IncludeCategory(string category)
        {
            if (string.IsNullOrEmpty(categories))
            {
                categories = string.Format(CultureInfo.InvariantCulture, "cat == {0}", category);
            }
            else
            {
                categories = string.Format(CultureInfo.InvariantCulture, "{0} || cat == {1}", categories, category);
            }

            return this;
        }

        /// <summary>
        /// Sets the .NET framework NUnit console should run under. Supported only in nunit v3 and above. For v2 use <see cref="AddNunitCommandLineOption"/>
        /// </summary>
        /// <param name="framework">Targeted .net framework</param>
        /// <returns>The NunitTask</returns>
        public NUnitTask SetTargetFramework(string framework)
        {
            targetFramework = framework;
            return this;
        }

        public NUnitTask SetWorkingDirectory(string directory)
        {
            workingDirectory = directory;
            return this;
        }

        /// <summary>
        ///  Add nunit command line option. Can be used multiple times.
        /// </summary>
        /// <param name="nunitCmdOption">nunit command line option to be added.</param>
        /// <returns>The NunitTask</returns>
        public NUnitTask AddNunitCommandLineOption(string nunitCmdOption)
        {
            nunitCommandLineOptions.Add(nunitCmdOption);
            return this;
        }

        /// <summary>
        /// Abstract method defining the actual work for a task.
        /// </summary>
        /// <remarks>This method has to be implemented by the inheriting task.</remarks>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute(ITaskContext context)
        {
            if (nunitConsoleFileName == null)
            {
                nunitConsoleFileName = context.Properties.Get<string>(BuildProps.NUnitConsolePath);
            }

            RunProgramTask task = new RunProgramTask(nunitConsoleFileName, false);

            SetAssemblyFileNameAndWorkingDirFromProjectName(context);
            Validate();

            task
                .SetWorkingDir(workingDirectory)
                .EncloseParametersInQuotes(false)
                .AddArgument(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", testAssemblyFileName));

            foreach (var nunitCommandLineOption in nunitCommandLineOptions)
            {
                task.AddArgument(nunitCommandLineOption);
            }

            if (!string.IsNullOrEmpty(targetFramework))
                task.AddArgument("/framework:{0}", targetFramework);

            if (!string.IsNullOrEmpty(categories))
                task.AddArgument("--where \"{0}\"", categories);
            
            task.Execute(context);
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(nunitConsoleFileName))
            {
                throw new TaskExecutionException("Nunit console file name is not set. Set it through constructor or build properties.");
            }

            if (!string.IsNullOrEmpty(categories))
            {
                if (nunitCommandLineOptions.Any(nunitCommandLineOption => nunitCommandLineOption.Contains("where")))
                {
                    throw new TaskExecutionException(
                        "Mixing Exclude/Include category with where clause is nunitCommandLineOptions is not supported eiter use exlude/include category or NunitCommandLineOption with where clause.");
                }
            }
        }

        private void SetAssemblyFileNameAndWorkingDirFromProjectName(ITaskContext context)
        {
            if (projectName != null)
            {
                VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);
                string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);

                VSProjectWithFileInfo project =
                    (VSProjectWithFileInfo)solution.FindProjectByName(projectName);
                FileFullPath projectTarget = project.ProjectDirectoryPath.CombineWith(project.GetProjectOutputPath(buildConfiguration))
                    .AddFileName("{0}.dll", project.ProjectName);

                testAssemblyFileName = projectTarget.ToString();
                workingDirectory = Path.GetDirectoryName(projectTarget.ToString());
            }
        }

        private string nunitConsoleFileName;

        /// <summary>
        /// unit test working directory.
        /// </summary>
        private string workingDirectory;

        /// <summary>
        ///  assembly to test.
        /// </summary>
        private string testAssemblyFileName;

        /// <summary>
        ///  test categories that will be included/excluded in tests.
        /// </summary>
        private string categories;

        /// <summary>
        /// .NET framework NUnit console should run under.
        /// </summary>
        private string targetFramework;

        private readonly List<string> nunitCommandLineOptions = new List<string>();

        private readonly string projectName = null;
    }
}