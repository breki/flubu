using System;
using Flubu;
using Flubu.Builds;
using Flubu.Targeting;
using Flubu.Tasks.FileSystem;

//css_ref bin\Debug\Flubu.dll;

namespace BuildScripts
{
    public class BuildScript
    {
        public static int Main(string[] args)
        {
            TargetTree targetTree = new TargetTree();
            BuildTargets.FillBuildTargets(targetTree);

            //targetTree.AddTarget("unit.tests")
            //    .SetDescription("Runs unit tests on the project")
            //    .Do(TargetRunTests).DependsOn("load.solution");
            //targetTree.AddTarget("package")
            //    .SetDescription("Packages all the build products into ZIP files")
            //    .Do(TargetPackage).DependsOn("load.solution");
            targetTree.AddTarget("rebuild")
                .SetDescription("Rebuilds the project, runs tests and packages the build products.")
                .SetAsDefault().DependsOn("compile");//, "unit.tests", "package");

            using (TaskContext context = new TaskContext(new SimpleTaskContextProperties()))
            using (ITaskSession taskSession = new TaskSession())
            {
                context.AddLogger(new MulticoloredConsoleLogger(Console.Out));

                context.Properties.Set(BuildProps.ProductId, "Flubu");
                context.Properties.Set(BuildProps.ProductName, "Flubu");
                context.Properties.Set(BuildProps.SolutionFileName, "Flubu.sln");
                context.Properties.Set(BuildProps.BuildConfiguration, "Release");
                context.Properties.Set(BuildProps.TargetDotNetVersion, FlubuEnvironment.Net35VersionNumber);
                context.Properties.Set(BuildProps.BuildDir, "BuildDir");

                try
                {
                    // actual run
                    if (args.Length == 0)
                        targetTree.RunTarget(context, targetTree.DefaultTarget.TargetName);
                    else
                    {
                        string targetName = args[0];
                        if (false == targetTree.HasTarget(targetName))
                        {
                            context.WriteError("ERROR: The target '{0}' does not exist", targetName);
                            targetTree.RunTarget(context, "help");
                            return 2;
                        }

                        targetTree.RunTarget(context, args[0]);
                    }

                    taskSession
                        .Complete();

                    return 0;
                }
                catch (RunnerFailedException)
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

        private static void TargetPackage(ITaskContext context)
        {
        //    runner
        //        .BuildProducts
        //            .AddProject("accipio", "Accipio.Console", false)
        //            .AddProject("flubu", "Flubu", String.Empty, true);
        //    runner
        //        .PackageBuildProduct("Accipio-{1}.zip", "Accipio-{1}", "accipio")
        //        .CopyBuildProductToCCNet(@"packages\Accipio\Accipio-latest.zip")
        //        .CopyBuildProductToCCNet(@"packages\Accipio\{2}.{3}\{4}")

        //        .PackageBuildProduct("Flubu-{1}.zip", "Flubu-{1}", "flubu")
        //        .CopyBuildProductToCCNet(@"packages\Flubu\Flubu-latest.zip")
        //        .CopyBuildProductToCCNet(@"packages\Flubu\{2}.{3}\{4}");
        }

        private static void TargetRunTests(ITaskContext context)
        {
            //x => x.RunTests("ProjectPilot.Tests", false);
        }
    }
}
