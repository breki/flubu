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
        public void ExactToolsVersionWasFound()
        {
            IDictionary<Version, string> msbuilds = new SortedDictionary<Version, string>();
            msbuilds.Add(new Version("2.0"), "somewhere2.0");
            msbuilds.Add(new Version("4.0"), "somewhere4.0");
            msbuilds.Add(new Version("12.0"), "somewhere12.0");

            const string MSBuildPath = @"somewhere4.0\MSBuild.exe";

            environMock.Setup(x => x.ListAvailableMSBuildToolsVersions()).Returns(msbuilds);
            runProgramTask = new RunProgramTask (MSBuildPath);
            runProgramTask.ProcessRunner = processRunnerMock.Object;

            const string ExpectedArgs = "\"x.sln\" \"/p:Configuration=Release\" \"/p:Platform=Any CPU\" \"/consoleloggerparameters:NoSummary\" \"/maxcpucount:3\" ";

            processRunnerMock
                .Setup(x => x.Run(MSBuildPath, ExpectedArgs, ".", null, It.IsNotNull<DataReceivedEventHandler>(), It.IsNotNull<DataReceivedEventHandler>()))
                .Returns(0).Verifiable();

            tasksFactoryMock.Setup(x => x.CreateRunProgramTask(MSBuildPath, false)).Returns(runProgramTask).Verifiable();

            task = new CompileSolutionTask("x.sln", "Release");
            task.ToolsVersion = new Version("4.0");
            task.CommonTasksFactory = tasksFactoryMock.Object;
            task.FlubuEnvironmentService = environMock.Object;
            task.UseSolutionDirAsWorkingDir = false;
            task.Execute(taskContext);

            tasksFactoryMock.Verify();
            processRunnerMock.Verify();
        }

        [SetUp]
        public void Setup ()
        {
            taskContext = new TaskContext (null, Enumerable.Empty<string> ());
            tasksFactoryMock = new Mock<ICommonTasksFactory>();
            environMock = new Mock<IFlubuEnvironmentService>();
            processRunnerMock = new Mock<IProcessRunner>();
        }

        private CompileSolutionTask task;
        private Mock<IFlubuEnvironmentService> environMock;
        private Mock<ICommonTasksFactory> tasksFactoryMock;
        private Mock<IProcessRunner> processRunnerMock;
        private ITaskContext taskContext;
        private IRunProgramTask runProgramTask;
    }
}