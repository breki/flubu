using System;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Packaging;
using Flubu.Targeting;

//css_ref Flubu.dll;
//css_ref Flubu.Contrib.dll;

namespace BuildScripts
{
    public class BuildScript
    {
        public static int Main(string[] args)
        {
            TargetTree targetTree = new TargetTree();
            BuildTargets.FillBuildTargets(targetTree);
            int testsRunCounter = 0;

            targetTree.AddTarget("unit.tests")
                .SetDescription("Runs unit tests on the project")
                .Do(x => BuildTargets.TargetRunTests(x, "Flubu.Tests", null, ref testsRunCounter)).DependsOn("load.solution");
            targetTree.AddTarget("package")
                .SetDescription("Packages all the build products into ZIP files")
                .Do(TargetPackage).DependsOn("load.solution");
            targetTree.AddTarget("rebuild")
                .SetDescription("Rebuilds the project, runs tests and packages the build products.")
                .SetAsDefault().DependsOn("compile", "fxcop", "unit.tests", "package");

            targetTree.GetTarget("fetch.build.version")
                .Do(TargetFetchBuildVersion);

            using (TaskSession session = new TaskSession(new SimpleTaskContextProperties(), args, targetTree))
            {
                BuildTargets.FillDefaultProperties(session);
                //session.Properties.Set (BuildProps.TargetDotNetVersion, FlubuEnvironment.Net20VersionNumber);

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

        private static void TargetFetchBuildVersion(ITaskContext context)
        {
            Version version = BuildTargets.FetchBuildVersionFromFile(context);
            version = new Version(version.Major, version.Minor, BuildTargets.FetchBuildNumberFromFile(context));
            context.Properties.Set(BuildProps.BuildVersion, version);
            context.WriteInfo("The build version will be {0}", version);
        }

        private static void TargetPackage(ITaskContext context)
        {
            FullPath zipPackagePath = new FullPath(context.Properties.Get(BuildProps.ProductRootDir, "."));
            zipPackagePath = zipPackagePath.CombineWith(context.Properties.Get<string>(BuildProps.BuildDir));
            FileFullPath zipFileName = zipPackagePath.AddFileName(
                "Flubu-{0}.zip", 
                context.Properties.Get<Version>(BuildProps.BuildVersion));

            StandardPackageDef packageDef = new StandardPackageDef("Flubu", context);
            VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);

            VSProjectWithFileInfo projectInfo = 
                (VSProjectWithFileInfo)solution.FindProjectByName("Flubu.Contrib");
            LocalPath projectOutputPath = projectInfo.GetProjectOutputPath(
                context.Properties.Get<string>(BuildProps.BuildConfiguration));
            FullPath projectTargetDir = projectInfo.ProjectDirectoryPath.CombineWith(projectOutputPath);
            packageDef.AddFolderSource(
                "bin", 
                projectTargetDir, 
                false);

            Zipper zipper = new Zipper(context);
            ZipProcessor zipProcessor = new ZipProcessor(
                context,
                zipper,
                zipFileName,
                projectTargetDir,
                null,
                "bin");
            zipProcessor.Process(packageDef);

            BuildTargets.IncrementBuildNumberInFile(context);
        }
    }
}
