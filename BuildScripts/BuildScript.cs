using System;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.Tasks.NuGetTasks;
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
            DefaultBuildScriptRunner runner = new DefaultBuildScriptRunner(ConfigureTargets, ConfigureBuildProperties);
            return runner.Run(args);
        }

        private static void ConfigureTargets(TargetTree targetTree)
        {
            targetTree.AddTarget("unit.tests")
                .SetDescription("Runs unit tests on the project")
                .Do(x => BuildTargets.TargetRunTestsNUnit(x, "Flubu.Tests")).DependsOn("load.solution");
            targetTree.AddTarget("package")
                .SetDescription("Packages all the build products into ZIP files")
                .Do(TargetPackage).DependsOn("load.solution");
            targetTree.AddTarget("rebuild")
                .SetDescription("Rebuilds the project, runs tests and packages the build products.")
                .SetAsDefault()
                .DependsOn("compile", "unit.tests", "package");
            targetTree.AddTarget("rebuild.server")
                .SetDescription(
                    "Rebuilds the project, runs tests, packages the build products and publishes it on the NuGet server.")
                .DependsOn("rebuild", "nuget");

            targetTree.GetTarget("fetch.build.version")
                .Do(TargetFetchBuildVersion);

            targetTree.AddTarget("nuget")
                .SetDescription("Produces NuGet packages for reusable components and publishes them to the NuGet server")
                .Do(c => TargetNuGet(c, "Flubu")).DependsOn("fetch.build.version");
        }

        private static void ConfigureBuildProperties(TaskSession session)
        {
            session.Properties.Set (BuildProps.NUnitConsolePath, @"packages\NUnit.Runners.2.6.2\tools\nunit-console.exe");
            session.Properties.Set (BuildProps.ProductId, "Flubu");
            session.Properties.Set (BuildProps.ProductName, "Flubu");
            session.Properties.Set (BuildProps.SolutionFileName, "Flubu.sln");
            session.Properties.Set (BuildProps.TargetDotNetVersion, FlubuEnvironment.Net40VersionNumber);
            session.Properties.Set (BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);
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
            PublishNuGetPackageTask publishTask = new PublishNuGetPackageTask(nugetId);
            publishTask.Execute(context);
        }
    }
}
