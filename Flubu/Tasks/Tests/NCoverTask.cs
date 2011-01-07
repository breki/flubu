using System;
using System.Globalization;
using System.IO;
using System.Text;
using Flubu.Tasks.Processes;
using Flubu.Tasks.Registry;

namespace Flubu.Tasks.Tests
{
    /// <summary>
    /// Run tests with coverage (NCover).
    /// <example>
    ///   NCoverTask task = new NCoverTask(Path.Combine("Testing\\UnitTests\\bin", BuildConfiguration),
    ///            "Hsl.PD.UnitTests.dll")
    ///        {
    ///            NCoverExecutablePath = MakePathFromRootDir(@"lib\NCover\NCover.Console.exe"),
    ///            TestToolPath = @"..\..\..\..\lib\NUnit\bin\net-2.0\nunit-console-x86.exe",
    ///            ExcludeCategories = "Cassini, LongTest",
    ///            TestToolType = UnitTestToolType.NUnit
    ///        };
    ///        task.Execute(ScriptExecutionEnvironment);
    /// </example>
    /// </summary>
    public class NCoverTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NCoverTask"/> class.
        /// </summary>
        /// <param name="workingDirectory">Working directory for tests.</param>
        /// <param name="assemblyToTest">Assembly to test.</param>
        public NCoverTask(string workingDirectory, string assemblyToTest)
        {
            WorkingDirectory = workingDirectory;
            AssemblyToTest = assemblyToTest;
            NCoverExecutablePath = ".\\lib\\NCover\\NCover.Console.exe";
            NCoverRoot = Path.GetDirectoryName(NCoverExecutablePath);
            TestToolPath = @"lib\NUnit\bin\net-2.0\nunit-console-x86.exe";
            TestToolType = UnitTestToolType.NUnit;
            OutputDirectory = workingDirectory;
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
        /// Gets or sets unit test tool path. (Default: lib\NUnit\bin\net-2.0\nunit-console-x86.exe)
        /// </summary>
        public string TestToolPath { get; set; }

        /// <summary>
        /// Gets or sets Full NCover test tool path. (Default: .\\lib\\NCover\\NCover.Console.exe)
        /// </summary>
        public string NCoverExecutablePath { get; set; }

        /// <summary>
        /// Gets or sets Unit test tool type. (Default: NUnit)
        /// </summary>
        public UnitTestToolType TestToolType { get; set; }

        /// <summary>
        /// Gets or sets excluded categories for NUnit. Only applies when TestToolType is set to NUnit.
        /// </summary>
        public string ExcludeCategories { get; set; }

        /// <summary>
        /// Gets or sets test fixtures to run (Fixture1, Fixture2, Fixture2). Only applies when TestToolType is set to Gallio.
        /// </summary>
        public string FixtureToRun { get; set; }

        /// <summary>
        /// Gets or sets tests to run (Test1, Test2, Test3).. Only applies when TestToolType is set to Gallio.
        /// </summary>
        public string TestToRun { get; set; }

        /// <summary>
        /// Gets or sets output directory.
        /// </summary>
        public string OutputDirectory { get; set; }

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
                    "Execute unit tests with NCover. Assembly:{0}",
                    AssemblyToTest);
            }
        }

        /// <summary>
        /// Abstract method defining the actual work for a task.
        /// </summary>
        /// <remarks>This method has to be implemented by the inheriting task.</remarks>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute(ITaskContext context)
        {
            string ncoverdll = Path.Combine(NCoverRoot, "CoverLib.dll");
            bool shouldRegister = ShouldRegisterNCover(context);

            if (shouldRegister)
            {
                RunProgramTask regTask = new RunProgramTask("regsvr32.exe", "/s " + ncoverdll, new TimeSpan(0, 0, 0, 10));
                regTask.Execute(context);
            }

            StringBuilder args = new StringBuilder();
            args.AppendFormat("//l {0} ", Path.Combine(OutputDirectory, "Coverage.log"));
            args.AppendFormat("//x {0} ", Path.Combine(OutputDirectory, "Coverage.xml"));
            args.AppendFormat("//a {0} ", AssemblyToTest);
            args.AppendFormat("{0} ", TestToolPath);
            args.AppendFormat("{0} ", AssemblyToTest);

            if (TestToolType == UnitTestToolType.NUnit && !string.IsNullOrEmpty(ExcludeCategories))
            {
                args.AppendFormat("\"/exclude={0}\" ", ExcludeCategories);
            }

            if (TestToolType == UnitTestToolType.NUnit)
            {
                args.AppendFormat("\"/xml={0}\" ", Path.Combine(OutputDirectory, "NUnitReport.xml"));
            }

            if (TestToolType == UnitTestToolType.Gallio)
            {
                string filter = null;
                if (!string.IsNullOrEmpty(FixtureToRun))
                    filter = "ExactType: " + FixtureToRun;

                if (!string.IsNullOrEmpty(TestToRun))
                {
                    if (!string.IsNullOrEmpty(filter))
                        filter += " and ";

                    filter += "Member: " + TestToRun;
                }

                if (!string.IsNullOrEmpty(filter))
                    args.AppendFormat("\"/f:{0}\" ", filter);
            }

            RunProgramTask task = new RunProgramTask(NCoverExecutablePath, args.ToString(), new TimeSpan(0, 1, 0, 0))
                       {
                           WorkingDirectory = WorkingDirectory
                       };
            task.Execute(context);

            if (shouldRegister)
            {
                task = new RunProgramTask("regsvr32.exe", "/s /u " + ncoverdll, new TimeSpan(0, 0, 0, 10));
                task.Execute(context);
            }
        }

        private static bool ShouldRegisterNCover(ITaskContext environment)
        {
            GetRegistryValueTask task = new GetRegistryValueTask(
                Microsoft.Win32.Registry.ClassesRoot,
                "TypeLib\\{3FB1CC1E-1C17-4A37-9C18-BF3DB8F10E46}\\1.0",
                string.Empty,
                "NCoverRegistered");

            try
            {
                task.Execute(environment);
            }
            catch (RunnerFailedException)
            {
                return true;
            }

            string val = environment.Properties.Get<string>("NCoverRegistered");

            return string.IsNullOrEmpty(val);
        }

        private string NCoverRoot { get; set; }
    }
}
