using System;
using System.Globalization;
using System.Text;
using Flubu.Tasks.Processes;

namespace Flubu.Tasks.Tests
{
    /// <summary>
    /// Task for running unit tests with gallio echo runner.
    /// <example>
    /// GallioUnitTestTask task = new GallioUnitTestTask(
    ///            Path.Combine("Hsl.PushDispatcherTest.SystemTest", GetProjectOutputPath("Hsl.PushDispatcherTest.SystemTest")),
    ///            "Hsl.PushDispatcherTest.SystemTest.dll")
    ///        {
    ///            FixtureToRun = fixture,
    ///            TestToRun = test
    ///        };
    ///        task.Execute(ScriptExecutionEnvironment);
    /// </example>
    /// </summary>
    public class GallioUnitTestTask : TaskBase
    {
        public static void Execute(ITaskContext environment, string workingFolder, string assemblyToTest)
        {
            GallioUnitTestTask task = new GallioUnitTestTask(workingFolder, assemblyToTest);
            task.Execute(environment);
        }

        public static void Execute(ITaskContext environment, string workingFolder, string assemblyToTest, string fixtureToRun, string testToRun)
        {
            GallioUnitTestTask task = new GallioUnitTestTask(workingFolder, assemblyToTest)
                                          {
                                              FixtureToRun = fixtureToRun,
                                              TestToRun = testToRun
                                          };
            task.Execute(environment);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GallioUnitTestTask"/> class.
        /// </summary>
        /// <param name="workingDirectory">Working directory for tests.</param>
        /// <param name="assemblyToTest">Assembly to test.</param>
        public GallioUnitTestTask(string workingDirectory, string assemblyToTest)
        {
            GallioEchoPath = ".\\lib\\Gallio\\bin\\Gallio.Echo.exe";
            AssemblyToTest = assemblyToTest;
            WorkingDirectory = workingDirectory;
        }

        /// <summary>
        /// Gets or sets unit test working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the report directory.
        /// </summary>
        /// <value>The report directory.</value>
        public string ReportDirectory
        {
            get { return reportDirectory; }
            set { reportDirectory = value; }
        }

        /// <summary>
        /// Gets or sets assembly to test.
        /// </summary>
        public string AssemblyToTest { get; set; }

        /// <summary>
        /// Gets or sets full path to Gallio.Echo.exe (.\lib\Gallio\bin\Gallio.Echo.exe)
        /// </summary>
        public string GallioEchoPath { get; set; }

        /// <summary>
        /// Gets or sets test fixtures to run. (Fixture1, Fixture2, Fixture2)
        /// </summary>
        public string FixtureToRun { get; set; }

        /// <summary>
        /// Gets or sets tests to run. (Test1, Test2, Test3)
        /// </summary>
        public string TestToRun { get; set; }

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
                  "Execute MbUnit unit test. Assembly:{0}",
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
            StringBuilder argumentLineBuilder = new StringBuilder();
            argumentLineBuilder.AppendFormat("\"{0}\" ", AssemblyToTest);
            argumentLineBuilder.AppendFormat("\"{0}\" ", "/verbosity:debug");
            argumentLineBuilder.AppendFormat("\"{0}\" ", "/rt:Html");
            argumentLineBuilder.AppendFormat("\"{0}\" ", "/rt:Xml");
            argumentLineBuilder.Append("\"/rnf:LastTestResults\" ");
            argumentLineBuilder.AppendFormat("\"/rd:{0}\" ", ReportDirectory);

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
                argumentLineBuilder.AppendFormat("\"/f:{0}\" ", filter);

            RunProgramTask task = new RunProgramTask(GallioEchoPath, argumentLineBuilder.ToString(), new TimeSpan(0, 1, 0, 0))
                                      {
                                          WorkingDirectory = WorkingDirectory,
                                          FailOnError = false,
                                      };
            task.Execute(context);
        }

        private string reportDirectory = "Reports";
    }
}
