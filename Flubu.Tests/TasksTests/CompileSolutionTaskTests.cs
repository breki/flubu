using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Flubu.Builds.Tasks.SolutionTasks;
using Flubu.Services;
using Flubu.Tasks.Processes;
using Moq;
using NUnit.Framework;

namespace Flubu.Tests.TasksTests
{
    public class CompileSolutionTaskTests
    {
        [Test]
        public void IfToolsVersionIsNotSpecifiedUseHighestOne()
        {
            SetupMSBuildVersions();

            const string MSBuildPath = @"somewhere12.0\MSBuild.exe";
            SetupRunProgramTask (MSBuildPath);

            task = new CompileSolutionTask("x.sln", "Release");
            task.CommonTasksFactory = tasksFactoryMock.Object;
            task.FlubuEnvironmentService = environMock.Object;
            task.UseSolutionDirAsWorkingDir = false;
            task.Execute(taskContext);

            tasksFactoryMock.Verify();
            processRunnerMock.Verify();
        }

        [Test]
        public void ExactToolsVersionWasFound()
        {
            SetupMSBuildVersions();

            const string MSBuildPath = @"somewhere4.0\MSBuild.exe";
            SetupRunProgramTask (MSBuildPath);

            task = new CompileSolutionTask("x.sln", "Release");
            task.ToolsVersion = new Version("4.0");
            task.CommonTasksFactory = tasksFactoryMock.Object;
            task.FlubuEnvironmentService = environMock.Object;
            task.UseSolutionDirAsWorkingDir = false;
            task.Execute(taskContext);

            tasksFactoryMock.Verify();
            processRunnerMock.Verify();
        }

        [Test]
        public void ToolsVersionWasNotFoundButThereIsNewerOne()
        {
            SetupMSBuildVersions (include40: false);

            const string MSBuildPath = @"somewhere12.0\MSBuild.exe";
            SetupRunProgramTask(MSBuildPath);

            task = new CompileSolutionTask ("x.sln", "Release");
            task.ToolsVersion = new Version ("4.0");
            task.CommonTasksFactory = tasksFactoryMock.Object;
            task.FlubuEnvironmentService = environMock.Object;
            task.UseSolutionDirAsWorkingDir = false;
            task.Execute (taskContext);

            tasksFactoryMock.Verify ();
            processRunnerMock.Verify ();
        }

        [Test]
        public void ToolsVersionWasNotFoundAndThereIsNoNewerOne()
        {
            SetupMSBuildVersions (include40: false, include120: false);

            task = new CompileSolutionTask ("x.sln", "Release");
            task.ToolsVersion = new Version ("4.0");
            task.CommonTasksFactory = tasksFactoryMock.Object;
            task.FlubuEnvironmentService = environMock.Object;
            task.UseSolutionDirAsWorkingDir = false;
            TaskExecutionException ex = Assert.Throws<TaskExecutionException>(() => task.Execute(taskContext));
            Assert.AreEqual(
                "Requested MSBuild tools version 4.0 not found and there are no higher versions",
                ex.Message);

            tasksFactoryMock.Verify ();
            processRunnerMock.Verify ();
        }

        [SetUp]
        public void Setup ()
        {
            taskContext = new TaskContext (null, Enumerable.Empty<string> ());
            tasksFactoryMock = new Mock<ICommonTasksFactory>();
            environMock = new Mock<IFlubuEnvironmentService>();
            processRunnerMock = new Mock<IProcessRunner>();
        }

        private void SetupMSBuildVersions(bool include40 = true, bool include120 = true)
        {
            IDictionary<Version, string> msbuilds = new SortedDictionary<Version, string>();
            msbuilds.Add(new Version("2.0"), "somewhere2.0");
            if (include40)
                msbuilds.Add(new Version("4.0"), "somewhere4.0");
            if (include120)
                msbuilds.Add(new Version("12.0"), "somewhere12.0");

            environMock.Setup(x => x.ListAvailableMSBuildToolsVersions()).Returns(msbuilds);
        }

        private void SetupRunProgramTask(string msBuildPath)
        {
            runProgramTask = new RunProgramTask(msBuildPath);
            runProgramTask.ProcessRunner = processRunnerMock.Object;

            const string ExpectedArgs = "\"x.sln\" \"/p:Configuration=Release\" \"/p:Platform=Any CPU\" \"/consoleloggerparameters:NoSummary\" \"/maxcpucount:3\" ";

            processRunnerMock.Setup(x => x.Run(msBuildPath, ExpectedArgs, ".", null, It.IsNotNull<DataReceivedEventHandler>(), It.IsNotNull<DataReceivedEventHandler>())).Returns(0).Verifiable();

            tasksFactoryMock.Setup(x => x.CreateRunProgramTask(msBuildPath, false)).Returns(runProgramTask).Verifiable();
        }

        private CompileSolutionTask task;
        private Mock<IFlubuEnvironmentService> environMock;
        private Mock<ICommonTasksFactory> tasksFactoryMock;
        private Mock<IProcessRunner> processRunnerMock;
        private ITaskContext taskContext;
        private IRunProgramTask runProgramTask;
    }
}