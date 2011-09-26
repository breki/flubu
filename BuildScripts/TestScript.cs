using System;
using Flubu;
using Flubu.Builds;
using Flubu.Targeting;
using Flubu.Tasks.Iis;

//css_ref Flubu.dll;
//css_ref Flubu.Contrib.dll;

namespace BuildScripts
{
    public class TestScript
    {
        public static int Main(string[] args)
        {
            TargetTree targetTree = new TargetTree();
            //BuildTargets.FillBuildTargets(targetTree);

            targetTree
                .AddTarget("iis7.binding")
                .Do(TargetAddIis7Binding);

            using (TaskSession session = new TaskSession(new SimpleTaskContextProperties(), args, targetTree))
            {
                BuildTargets.FillDefaultProperties(session);
                session.Start(BuildTargets.OnBuildFinished);

                session.AddLogger(new MulticoloredConsoleLogger(Console.Out));

                session.Properties.Set(BuildProps.ProductId, "Flubu");
                session.Properties.Set(BuildProps.ProductName, "Flubu");
                session.Properties.Set(BuildProps.SolutionFileName, "Flubu.sln");
                session.Properties.Set(BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);

                try
                {
                    // actual run
                    if (args.Length == 0)
                        targetTree.RunTarget(session, targetTree.DefaultTarget.TargetName);
                    else
                    {
                        string targetName = args[0];
                        if (false == targetTree.HasTarget(targetName))
                        {
                            session.WriteError("ERROR: The target '{0}' does not exist", targetName);
                            targetTree.RunTarget(session, "help");
                            return 2;
                        }

                        targetTree.RunTarget(session, args[0]);
                    }

                    session
                        .Complete();

                    return 0;
                }
                catch (TaskExecutionException)
                {
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 1;
                }
            }
        }

        private static void TargetAddIis7Binding(ITaskContext context)
        {
            var master = new IisMaster(context);
            IIisTasksFactory factory = master.LocalIisTasksFactory;
            IAddWebSiteBindingTask controlWebSiteTask = factory.AddWebSiteBindingTask;
            controlWebSiteTask
                .SiteName("Default Web Site")
                .AddBinding("https")
                .Execute(context);
        }
    }
}
