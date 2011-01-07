using System;
using Flubu.Targeting;
using Flubu.Tasks.UserInterface;
using MbUnit.Framework;

namespace Flubu.Tests
{
    public class BasicTests
    {
        [Test]
        public void Test()
        {
            TargetTree targetTree = new TargetTree();
            targetTree.AddTarget("compile")
                .SetDescription("Compiles the VS solution and runs FxCop analysis on it")
                .Do(TargetCompile).DependsOn("load.solution");

            ITarget target = targetTree.GetTarget("compile");
            using (ITaskSession session = new TaskSession(new SimpleTaskContextProperties()))
            {
                session.Start(OnBuildFinished);
                NotifyUserTask task = new NotifyUserTask("message");
                task.Execute(session);
            }
        }

        private static void OnBuildFinished(ITaskSession session)
        {
        }

        private void TargetCompile(ITaskContext obj)
        {
        }
    }
}