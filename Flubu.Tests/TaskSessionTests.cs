using Flubu.Targeting;
using NUnit.Framework;

namespace Flubu.Tests
{
    public class TaskSessionTests
    {
        [Test]
        public void ResettingSessionMeansTargetsCanBeReexecuted()
        {
            using (ITaskSession session = new TaskSession(properties, args, targetTree))
            {
                session.Start(x => Assert.IsFalse(x.HasFailed));

                targetTree.RunTarget(session, "build");
                session.Reset();
                targetTree.RunTarget (session, "build");

                session.Complete();
            }

            Assert.AreEqual(2, compilesCount);
            Assert.AreEqual(2, compilesCount);
            Assert.AreEqual(2, buildsCount);
        }

        [Test]
        public void ResettingSessionMeansPropertiesAreCleared()
        {
            using (ITaskSession session = new TaskSession(properties, args, targetTree))
            {
                session.Start(x => Assert.IsFalse(x.Properties.Has("somekey")));

                properties.Set("somekey", "somevalue");
                targetTree.RunTarget(session, "build");
                session.Reset();
                targetTree.RunTarget (session, "build");

                session.Complete();
            }

            Assert.AreEqual(2, compilesCount);
            Assert.AreEqual(2, compilesCount);
            Assert.AreEqual(2, buildsCount);
        }

        [SetUp]
        public void Setup()
        {
            compilesCount = testsCount = buildsCount = 0;

            properties = new SimpleTaskContextProperties();
            args = new string[0];
            
            targetTree = new TargetTree();
            targetTree.AddTarget("build").DependsOn("compile", "test").Do(x => buildsCount++);
            targetTree.AddTarget("compile").Do(x => compilesCount++);
            targetTree.AddTarget ("test").DependsOn ("compile").Do(x => testsCount++);
        }

        private int compilesCount;
        private int testsCount;
        private int buildsCount;
        private ITaskContextProperties properties;
        private string[] args;
        private TargetTree targetTree;
    }
}