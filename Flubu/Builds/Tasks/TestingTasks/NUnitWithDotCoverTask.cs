using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Flubu.Builds.Tasks.NuGetTasks;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Tasks.Processes;
using Flubu.Tasks.Text;

namespace Flubu.Builds.Tasks.TestingTasks
{
    /// <summary>
    /// Runs NUnit tests in combination with dotCover test coverage analysis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The task uses dotCover command line tool to run NUnit command line runner
    /// which executes tests for the specified assembly or C# project.
    /// </para>
    /// <para>
    /// The task uses <see cref="ForProject"/> to download dotCover command
    /// line tool into the running user's local application data directory. If the tool is already there,
    /// the task skips downloading it.
    /// </para>
    /// <para>
    /// Use one of provided static constructor methods: <see cref="ForAssembly"/> or <see cref="DownloadNugetPackageInUserRepositoryTask"/>.
    /// </para>
    /// </remarks>
    public class NUnitWithDotCoverTask : TaskBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="NUnitWithDotCoverTask"/> that will run the task
        /// for the specified C# project containing unit tests.
        /// </summary>
        /// <param name="projectName">The name of the C# project containing unit tests.</param>
        /// <param name="nunitRunnerFileName">The file path to NUnit's console runner.</param>
        /// <returns>The newly created <see cref="NUnitWithDotCoverTask"/> instance.</returns>
        /// <seealso cref="ForAssembly"/>
        public static NUnitWithDotCoverTask ForProject(string projectName, string nunitRunnerFileName)
        {
            if (string.IsNullOrEmpty(projectName))
                throw new ArgumentException("Project name should not be null or empty string", "projectName");

            return new NUnitWithDotCoverTask(null, projectName, nunitRunnerFileName);
        }

        /// <summary>
        /// Creates a new instance of <see cref="NUnitWithDotCoverTask"/> that will run the task
        /// for the specified assembly containing unit tests.
        /// </summary>
        /// <param name="assemblyFileName">The file path to the assembly containing unit tests.</param>
        /// <param name="nunitRunnerFileName">The file path to NUnit's console runner.</param>
        /// <returns>The newly created <see cref="NUnitWithDotCoverTask"/> instance.</returns>
        /// <seealso cref="ForProject"/>
        public static NUnitWithDotCoverTask ForAssembly (string assemblyFileName, string nunitRunnerFileName)
        {
            if (string.IsNullOrEmpty (assemblyFileName))
                throw new ArgumentException ("Assembly file name should not be null or empty string", "assemblyFileName");

            return new NUnitWithDotCoverTask (assemblyFileName, null, nunitRunnerFileName);
        }

        public override string Description
        {
            get
            {
                if (testAssemblyFileName != null)
                    return string.Format (
                        CultureInfo.InvariantCulture,
                        "Execute NUnit unit tests on assembly '{0}'",
                        testAssemblyFileName);

                return string.Format (
                    CultureInfo.InvariantCulture,
                    "Execute NUnit unit tests on project '{0}'",
                    projectName);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the build should fail if the test coverage of any
        /// class is below <see cref="MinRequiredCoverage"/>. 
        /// </summary>
        /// <remarks>
        /// If <see cref="FailBuildOnViolations"/>
        /// is set to <c>false</c>, the task will only print out information about violating classes
        /// without failing the build.
        /// The default value is <c>true</c>.
        /// </remarks>
        public bool FailBuildOnViolations
        {
            get { return failBuildOnViolations; }
            set { failBuildOnViolations = value; }
        }

        /// <summary>
        /// Gets or sets the minimum required test coverage percentage. 
        /// If any class has the test coverage below this value and <see cref="FailBuildOnViolations"/>
        /// is set to <c>true</c>, the task will fail the build. 
        /// </summary>
        /// <remarks>
        /// The default value is 75%.
        /// </remarks>
        public int MinRequiredCoverage
        {
            get { return minRequiredCoverage; }
            set { minRequiredCoverage = value; }
        }

        /// <summary>
        /// Gets or sets the command line options for NUnit console runner (as a single string).
        /// </summary>
        /// <remarks>
        /// The default options are <c>/labels /nodots</c>.
        /// </remarks>
        public string NUnitCmdLineOptions
        {
            get { return nunitCmdLineOptions; }
            set { nunitCmdLineOptions = value; }
        }

        /// <summary>
        /// Gets or sets the dotCover filters that will be passed to dotCover's <c>/Filters</c> command line parameter.
        /// </summary>
        /// <remarks>
        /// The default filters are set to <c>-:module=*.Tests;-:class=*Contract;-:class=*Contract`*</c>.
        /// For more information, visit <a href="https://www.jetbrains.com/dotcover/help/dotCover__Console_Runner_Commands.html">here</a>.
        /// </remarks>
        /// <seealso cref="DotCoverAttributeFilters"/>
        public string DotCoverFilters
        {
            get { return dotCoverFilters; }
            set { dotCoverFilters = value; }
        }

        /// <summary>
        /// Gets or sets the dotCover attribute filters that will be passed to dotCover's <c>/AttributeFilters</c> command line parameter.
        /// Attribute filters tell dotCover to skip the analysis of any code that has the specified attribute(s) applied.
        /// </summary>
        /// <remarks>
        /// The default attribute filters are set to <c>"*.ExcludeFromCodeCoverageAttribute"</c>.
        /// For more information, visit <a href="https://www.jetbrains.com/dotcover/help/dotCover__Console_Runner_Commands.html">here</a>.
        /// </remarks>
        /// <seealso cref="DotCoverFilters"/>
        public string DotCoverAttributeFilters
        {
            get { return dotCoverAttributeFilters; }
            set { dotCoverAttributeFilters = value; }
        }

        /// <summary>
        /// Gets the path to the generated dotCover test coverage XML report.
        /// </summary>
        /// <seealso cref="CoverageHtmlReportFileName"/>
        public string CoverageXmlReportFileName
        {
            get { return coverageXmlReportFileName; }
        }

        /// <summary>
        /// Gets the path to the generated dotCover test coverage HTML report.
        /// </summary>
        /// <seealso cref="CoverageXmlReportFileName"/>
        public string CoverageHtmlReportFileName
        {
            get { return coverageHtmlReportFileName; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            string dotCoverExeFileName;
            if (!EnsureDotCoverIsAvailable(context, out dotCoverExeFileName))
                return;

            string assemblyId;
            FileFullPath assemblyFullFileName = ExtractFullAssemblyFileName(context, out assemblyId);

            string buildDir = context.Properties[BuildProps.BuildDir];
            string snapshotFileName = Path.Combine (buildDir, "{0}-coverage.dcvr".Fmt (assemblyId));

            RunCoverTask(context, assemblyFullFileName, dotCoverExeFileName, snapshotFileName);

            coverageXmlReportFileName = GenerateCoverageReport(context, dotCoverExeFileName, snapshotFileName, "XML");
            coverageHtmlReportFileName = GenerateCoverageReport(context, dotCoverExeFileName, snapshotFileName, "HTML");

            AnalyzeCoverageResults(context);
        }

        private NUnitWithDotCoverTask(string testAssemblyFileName, string projectName, string nunitRunnerFileName)
        {
            if (string.IsNullOrEmpty (nunitRunnerFileName))
                throw new ArgumentException ("NUnit Runner file name should not be null or empty string", "nunitRunnerFileName");

            this.testAssemblyFileName = testAssemblyFileName;
            this.projectName = projectName;
            this.nunitRunnerFileName = nunitRunnerFileName;
        }

        private static bool EnsureDotCoverIsAvailable(ITaskContext context, out string dotCoverExeFileName)
        {
            const string DotCoverCmdLineToolsPackageId = "JetBrains.dotCover.CommandLineTools";

            DownloadNugetPackageInUserRepositoryTask downloadPackageTask =
                new DownloadNugetPackageInUserRepositoryTask(DotCoverCmdLineToolsPackageId);
            downloadPackageTask.Execute(context);

            dotCoverExeFileName = Path.Combine(downloadPackageTask.PackageDirectory, "tools/dotCover.exe");

            if (!File.Exists(dotCoverExeFileName))
            {
                context.Fail(
                    "R# dotCover is not present in the expected location ('{0}'), cannot run test coverage analysis",
                    dotCoverExeFileName);
                return false;
            }

            return true;
        }

        private void RunCoverTask(
            ITaskContext context,
            IPathBuilder assemblyFullFileName,
            string dotCoverExeFileName,
            string snapshotFileName)
        {
            string projectDir = Path.GetDirectoryName(assemblyFullFileName.ToString());
            string projectBinFileName = Path.GetFileName(assemblyFullFileName.FileName);

            context.WriteInfo("Running unit tests (with code coverage)...");
            RunProgramTask runDotCovertask = new RunProgramTask(dotCoverExeFileName).AddArgument("cover")
                .AddArgument("/TargetExecutable={0}", nunitRunnerFileName)
                .AddArgument("/TargetArguments={0} {1}", projectBinFileName, nunitCmdLineOptions)
                .AddArgument("/TargetWorkingDir={0}", projectDir)
                .AddArgument("/Filters={0}", dotCoverFilters)
                .AddArgument("/AttributeFilters={0}", dotCoverAttributeFilters)
                .AddArgument("/Output={0}", snapshotFileName)
                //.AddArgument("/LogFile={0}", Path.Combine(buildDir, "dotCover-log.xml"))
                .AddArgument("/ReturnTargetExitCode");
            runDotCovertask.Execute(context);
        }

        private static string GenerateCoverageReport(
            ITaskContext context,
            string dotCoverExeFileName,
            string snapshotFileName,
            string reportType)
        {
            context.WriteInfo ("Generating code coverage {0} report...", reportType);

            string buildDir = context.Properties[BuildProps.BuildDir];

            string coverageReportFileName = Path.Combine (buildDir, "dotCover-results.{0}".Fmt (reportType.ToLowerInvariant()));
            RunProgramTask runDotCovertask =
                new RunProgramTask(dotCoverExeFileName).AddArgument("report")
                    .AddArgument("/Source={0}", snapshotFileName)
                    .AddArgument("/Output={0}", coverageReportFileName)
                    .AddArgument ("/ReportType={0}", reportType)
                //.AddArgument("/LogFile={0}", Path.Combine(buildDir, "dotCover-log.xml"))
                ;
            runDotCovertask.Execute(context);
            return coverageReportFileName;
        }

        private FileFullPath ExtractFullAssemblyFileName(ITaskContext context, out string assemblyId)
        {
            FileFullPath assemblyFullFileName;

            if (projectName != null)
            {
                VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);
                string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);

                VSProjectWithFileInfo project = (VSProjectWithFileInfo)solution.FindProjectByName(projectName);
                assemblyFullFileName =
                    project.ProjectDirectoryPath.CombineWith(project.GetProjectOutputPath(buildConfiguration))
                        .AddFileName("{0}.dll", project.ProjectName);

                assemblyId = projectName;
            }
            else
            {
                assemblyFullFileName = new FileFullPath(testAssemblyFileName);
                assemblyId = Path.GetFileNameWithoutExtension(assemblyFullFileName.FileName);
            }

            return assemblyFullFileName;
        }

        private void AnalyzeCoverageResults(ITaskContext context)
        {
            const string PropertyTotalCoverage = "TotalTestCoverage";
            const string PropertyClassesWithPoorCoverageCount = "PoorCoverageCount";

            string totalCoverageExpression = "sum(/Root/Assembly[1]/@CoveragePercent)";
            string classesWithPoorCoverageExpression = string.Format(
                CultureInfo.InvariantCulture,
                "count(/Root/Assembly/Namespace/Type[@CoveragePercent<{0}])",
                minRequiredCoverage);

            EvaluateXmlTask countViolationsTask =
                new EvaluateXmlTask(coverageXmlReportFileName)
                    .AddExpression(
                        PropertyClassesWithPoorCoverageCount,
                        classesWithPoorCoverageExpression)
                    .AddExpression(
                        PropertyTotalCoverage,
                        totalCoverageExpression);
            countViolationsTask.Execute(context);

            int? totalCoverage = GetCoverageProperyValue(context, PropertyTotalCoverage);
            context.WriteInfo("Total test coverage is {0}%", totalCoverage);

            int? duplicatesCount = GetCoverageProperyValue(context, PropertyClassesWithPoorCoverageCount);
            if (duplicatesCount.HasValue && duplicatesCount > 0)
                FailBuildAndPrintOutCoverageReport(context, duplicatesCount);
        }

        private static int? GetCoverageProperyValue (ITaskContext context, string propertyName)
        {
            string valueStr = context.Properties[propertyName];
            if (valueStr == null)
                return null;

            return int.Parse (valueStr, CultureInfo.InvariantCulture);
        }

        private void FailBuildAndPrintOutCoverageReport(ITaskContext context, int? duplicatesCount)
        {
            context.WriteMessage(
                TaskMessageLevel.Warn,
                "There are {0} classes that have the test coverage below the minimum {1}% threshold",
                duplicatesCount,
                minRequiredCoverage);

            string classesWithPoorCoverageExpression = string.Format (
                CultureInfo.InvariantCulture,
                "/Root/Assembly/Namespace/Type[@CoveragePercent<{0}]",
                minRequiredCoverage);

            VisitXmlFileTask findViolationsTask = new VisitXmlFileTask(coverageXmlReportFileName);

            List<Tuple<string, int>> poorCoverageClasses = new List<Tuple<string, int>>();

            findViolationsTask.AddVisitor(
                classesWithPoorCoverageExpression,
                node =>
                    {
                        if (node.Attributes == null || node.ParentNode == null || node.ParentNode.Attributes == null)
                            return true;

                        string className = node.Attributes["Name"].Value;
                        string nspace = node.ParentNode.Attributes["Name"].Value;
                        int coverage = int.Parse(node.Attributes["CoveragePercent"].Value);

                        poorCoverageClasses.Add(new Tuple<string, int>(nspace + "." + className, coverage));
                        return true;
                    });
            findViolationsTask.Execute (context);

            poorCoverageClasses.Sort(ClassCoverageComparer);
            foreach (Tuple<string, int> tuple in poorCoverageClasses)
                context.WriteInfo("{0} ({1}%)", tuple.Item1, tuple.Item2);

            if (failBuildOnViolations)
                context.Fail("Failing the build because of poor test coverage");
        }

        private static int ClassCoverageComparer(Tuple<string, int> a, Tuple<string, int> b)
        {
            int c = a.Item2.CompareTo(b.Item2);
            if (c != 0)
                return c;

            return string.Compare(a.Item1, b.Item1, StringComparison.Ordinal);
        }

        private readonly string testAssemblyFileName;
        private readonly string projectName;
        private readonly string nunitRunnerFileName;
        private int minRequiredCoverage = 75;
        private string coverageXmlReportFileName;
        private string coverageHtmlReportFileName;
        private string dotCoverFilters = "-:module=*.Tests;-:class=*Contract;-:class=*Contract`*";
        private string dotCoverAttributeFilters = "*.ExcludeFromCodeCoverageAttribute";
        private string nunitCmdLineOptions = "/labels /nodots";
        private bool failBuildOnViolations = true;
    }
}