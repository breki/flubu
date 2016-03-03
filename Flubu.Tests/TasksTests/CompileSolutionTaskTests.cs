using System.Linq;
using Flubu.Builds.Tasks.SolutionTasks;
using Flubu.Services;
using Flubu.Tasks.Processes;
using NUnit.Framework;
using Rhino.Mocks;

namespace Flubu.Tests.TasksTests
{
    public class CompileSolutionTaskTests
    {
        [Test]
        public void Test()
        {
            task = new CompileSolutionTask("x.sln", "Release");
            task.CommonTasksFactory = tasksFactory;
            task.Execute(taskContext);
        }

        [SetUp]
        public void Setup ()
        {
            taskContext = new TaskContext (null, Enumerable.Empty<string> ());
            tasksFactory = MockRepository.GenerateStub<ICommonTasksFactory>();
            runProgramTask = MockRepository.GenerateMock<IRunProgramTask>();
        }

        private CompileSolutionTask task;
        private ICommonTasksFactory tasksFactory;
        private ITaskContext taskContext;
        private IRunProgramTask runProgramTask;
    }
}