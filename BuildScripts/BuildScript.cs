using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Packaging;
using Flubu.Targeting;
using Flubu.Tasks.Processes;
using Flubu.Tasks.Text;

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

            targetTree.AddTarget("unit.tests")
                .SetDescription("Runs unit tests on the project")
                .Do(x => BuildTargets.TargetRunTestsNUnit(x, "Flubu.Tests")).DependsOn("load.solution");
            targetTree.AddTarget("package")
                .SetDescription("Packages all the build products into ZIP files")
                .Do(TargetPackage).DependsOn("load.solution");
            targetTree.AddTarget("rebuild")
                .SetDescription("Rebuilds the project, runs tests and packages the build products.")
                .SetAsDefault().DependsOn("compile", "fxcop", "unit.tests", "package");

            targetTree.GetTarget("fetch.build.version")
                .Do(TargetFetchBuildVersion);

            targetTree.AddTarget("nuget")
                .SetDescription("Produces NuGet packages for reusable components and publishes them to the NuGet server")
                .Do(c => TargetNuGet(c, "Flubu")).DependsOn("fetch.build.version");

            using (TaskSession session = new TaskSession(new SimpleTaskContextProperties(), args, targetTree))
            {
                session.IsInteractive =
                    Environment.GetEnvironmentVariable("CI") == null
                    && Environment.GetEnvironmentVariable("APPVEYOR") == null
                    && Environment.GetEnvironmentVariable("BUILD_NUMBER") == null;

                BuildTargets.FillDefaultProperties(session);
                session.Start(BuildTargets.OnBuildFinished);

                session.AddLogger(new MulticoloredConsoleLogger(Console.Out));

                session.Properties.Set(BuildProps.NUnitConsolePath, @"packages\NUnit.Runners.2.6.2\tools\nunit-console.exe");
                session.Properties.Set(BuildProps.ProductId, "Flubu");
                session.Properties.Set(BuildProps.ProductName, "Flubu");
                session.Properties.Set(BuildProps.SolutionFileName, "Flubu.sln");
                session.Properties.Set(BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);

                try
                {
                    string targetToRun = ParseCmdLineArgs(args, session);
                    
                    if (targetToRun == null)
                        targetTree.RunTarget(session, targetTree.DefaultTarget.TargetName);
                    else
                    {
                        if (false == targetTree.HasTarget(targetToRun))
                        {
                            session.WriteError ("ERROR: The target '{0}' does not exist", targetToRun);
                            targetTree.RunTarget(session, "help");
                            return 2;
                        }

                        targetTree.RunTarget (session, targetToRun);
                    }

                    session.Complete();

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 1;
                }
            }
        }

        private static string ParseCmdLineArgs (IEnumerable<string> args, ITaskContext context)
        {
            string targetToBuild = null;

            foreach (string arg in args)
            {
                if (string.Compare (arg, "-speechdisabled", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    context.Properties.Set (BuildProps.SpeechDisabled, true);
                }
                else
                    targetToBuild = arg;
            }

            return targetToBuild;
        }

        private static void TargetPackage(ITaskContext context)
        {
            FullPath packagesDir = new FullPath(context.Properties.Get(BuildProps.ProductRootDir, "."));
            packagesDir = packagesDir.CombineWith(context.Properties.Get<string>(BuildProps.BuildDir));
            FullPath simplexPackageDir = packagesDir.CombineWith("Flubu");
            FileFullPath zipFileName = packagesDir.AddFileName(
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

            ICopier copier = new Copier(context);
            CopyProcessor copyProcessor = new CopyProcessor(
                 context,
                 copier,
                 simplexPackageDir);
            copyProcessor
                .AddTransformation("bin", new LocalPath(string.Empty));

            IPackageDef copiedPackageDef = copyProcessor.Process(packageDef);

            Zipper zipper = new Zipper(context);
            ZipProcessor zipProcessor = new ZipProcessor(
                context,
                zipper,
                zipFileName,
                simplexPackageDir,
                null,
                "bin");
            zipProcessor.Process(copiedPackageDef);
        }

        private static void TargetFetchBuildVersion(ITaskContext context)
        {
            Version version = BuildTargets.FetchBuildVersionFromFile(context);
            context.Properties.Set(BuildProps.BuildVersion, version);
            context.WriteInfo("The build version will be {0}", version);
        }

        private static void TargetNuGet(ITaskContext context, string nugetId)
        {
            FullPath packagesDir = new FullPath(context.Properties.Get(BuildProps.ProductRootDir, "."));
            packagesDir = packagesDir.CombineWith(context.Properties.Get<string>(BuildProps.BuildDir));

            string sourceNuspecFile = string.Format(
                CultureInfo.InvariantCulture,
                @"{0}\{0}.nuspec",
                nugetId);
            FileFullPath destNuspecFile = packagesDir.AddFileName("{0}.nuspec", nugetId);

            context.WriteInfo("Preparing the {0} file", destNuspecFile);
            ExpandPropertiesTask task = new ExpandPropertiesTask(
                sourceNuspecFile,
                destNuspecFile.ToString(),
                Encoding.UTF8,
                Encoding.UTF8);
            task.AddPropertyToExpand("version", context.Properties.Get<Version>(BuildProps.BuildVersion).ToString());
            task.Execute(context);

            // package it
            context.WriteInfo("Creating a NuGet package file");
            RunProgramTask progTask = new RunProgramTask(@"lib\NuGet\NuGet.exe");
            progTask.SetWorkingDir(destNuspecFile.Directory.ToString());
            progTask
                .AddArgument("pack")
                .AddArgument(destNuspecFile.FileName)
                .AddArgument("-Verbose")
                .Execute(context);

            string nupkgFileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}.nupkg",
                nugetId,
                context.Properties.Get<Version>(BuildProps.BuildVersion));
            context.WriteInfo("NuGet package file {0} created", nupkgFileName);

            string apiKeyFileName = "NuGet API key.txt";
            if (!File.Exists(apiKeyFileName))
            {
                context.WriteInfo("'NuGet API key.txt' does not exist, cannot publish the package.");
                return;
            }

            string apiKey = File.ReadAllText(apiKeyFileName);

            // publish the package file
            context.WriteInfo("Pushing the NuGet package to the repository");

            progTask = new RunProgramTask(@"lib\NuGet\NuGet.exe");
            progTask.SetWorkingDir(destNuspecFile.Directory.ToString());
            progTask
                .AddArgument("push")
                .AddArgument(nupkgFileName)
                .AddArgument(apiKey)
                .AddArgument("-Source")
                .AddArgument("http://packages.nuget.org/v1/")
                .Execute(context);
        }
    }
}
