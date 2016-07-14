using Flubu.Builds.Tasks.NuGetTasks;
using NUnit.Framework;

namespace Flubu.Tests.TasksTests
{
    public class FindNuGetPackageInUserRepositoryTaskTests
    {
        [Test]
        public void Test()
        {
            task.Execute(new TaskContext(new SimpleTaskContextProperties(), new string[] { }));    
        }

        [SetUp]
        public void Setup()
        {
            task = new FindNuGetPackageInUserRepositoryTask("JetBrains.dotCover.CommandLineTools");
        }

        private FindNuGetPackageInUserRepositoryTask task;
    }
}