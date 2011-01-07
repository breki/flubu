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
            using (TaskContext context = new TaskContext(new SimpleTaskContextProperties()))
            using (ITaskSession taskSession = new TaskSession())
            {
                taskSession.Start(context);
                NotifyUserTask task = new NotifyUserTask("message");
                task.Execute(context);
            }
        }

        private void TargetCompile(ITaskContext obj)
        {
        }
    }
}