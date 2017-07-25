using System;
using Flubu.Builds.Tasks.NuGetTasks;
using NUnit.Framework;

namespace Flubu.Tests.TasksTests
{
    public class PublishNuGetPackageTaskTests
    {
        [Test]
        public void ConstructProductVersionWhenProductVersionHasOnly3Fields()
        {
            Assert.AreEqual(
                "1.2.3",
                PublishNuGetPackageTask.ConstructProductVersionStringUsedForNupkg(
                    new Version("1.2.3")));
        }

        [Test]
        public void ConstructProductVersionWhenProductVersionRevisionIs0()
        {
            Assert.AreEqual(
                "1.2.3",
                PublishNuGetPackageTask.ConstructProductVersionStringUsedForNupkg(
                    new Version("1.2.3.0")));
        }

        [Test]
        public void ConstructProductVersionWhenProductVersionHasAll4Fields()
        {
            Assert.AreEqual(
                "1.2.3.4",
                PublishNuGetPackageTask.ConstructProductVersionStringUsedForNupkg(
                    new Version("1.2.3.4")));
        }
    }
}