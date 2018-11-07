using System;
using System.Collections.Generic;
using System.IO;
using Flubu;
using Flubu.Builds;
using Flubu.Builds.Tasks.NuGetTasks;
using Flubu.Builds.Tasks.TestingTasks;
using Flubu.Targeting;
using Flubu.Tasks.FileSystem;
using Flubu.Tasks.Processes;

namespace BuildScripts
{
    public class BuildScript : DefaultBuildScript
    {
        protected override void ConfigureBuildProperties(TaskSession session)
        {
            session.Properties.Set(
                BuildProps.MSBuildPath,
                @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\msbuild.exe");
            session.Properties.Set(
                BuildProps.NUnitConsolePath,
                @"packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe");
            session.Properties.Set(BuildProps.ProductId, "Flubu");
            session.Properties.Set(BuildProps.ProductName, "Flubu");
            session.Properties.Set(BuildProps.SolutionFileName, "Flubu.sln");
            session.Properties.Set(BuildProps.VersionControlSystem, VersionControlSystem.Mercurial);
        }

        protected override void ConfigureTargets(TargetTree targetTree, ICollection<string> args)
        {
            targetTree.AddTarget("unit.tests")
               .SetDescription("Runs unit tests on the project")
               .Do(x => TargetRunTests(x, "Flubu.Tests")).DependsOn("load.solution");
            targetTree.AddTarget("rebuild")
                .SetDescription("Rebuilds the project, runs tests and packages the build products.")
                .SetAsDefault()
                .DependsOn("compile", "unit.tests", "il.merge");
            targetTree.AddTarget("rebuild.server")
                .SetDescription(
                    "Rebuilds the project, runs tests, packages the build products and publishes it on the NuGet server.")
                .DependsOn("rebuild", "nuget");

            targetTree.GetTarget("fetch.build.version")
                .Do(TargetFetchBuildVersion);

            targetTree.AddTarget("nuget")
                .SetDescription("Produces NuGet packages for reusable components and publishes them to the NuGet server")
                .Do(c => TargetNuGet(c, "Flubu")).DependsOn("fetch.build.version");

            targetTree.AddTarget("il.merge")
                .DependsOn("load.solution")
                .Do(TargetIlMerge);
        }

        private static void TargetIlMerge(ITaskContext context)
        {
            CreateDirectoryTask.Execute(context, "./output", false);

            string projectTargetDir = Path.Combine(
                "Flubu.Console", "bin", context.Properties[BuildProps.BuildConfiguration]);

            IRunProgramTask progTask =
                new RunProgramTask(@"packages\ILRepack.2.0.16\tools\ILRepack.exe")
                    .SetWorkingDir(projectTargetDir);

            progTask
                .EncloseParametersInQuotes(false)
                .ExecutionTimeout(TimeSpan.FromSeconds(30))
                .AddArgument("/target:exe")
                .AddArgument("/xmldocs")
                //.AddArgument("/v4.6")
                //.AddArgument(@"/targetplatform:v4.6,""C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6""")
                //.AddArgument(@"/targetplatform:v4,""C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0""")
                //.AddArgument(@"/lib:""C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0""")
                .AddArgument("/out:..\\..\\..\\output\\Build.exe")
                .AddArgument("flubu.console.exe")
                .AddArgument("CommandLine.dll")
                .AddArgument("CSScriptLibrary.dll")
                .AddArgument("Flubu.dll")
                .AddArgument("Flubu.Contrib.dll")
                .AddArgument("ICSharpCode.SharpZipLib.dll")
                .AddArgument("Microsoft.Web.Administration.dll")
                .AddArgument("Mono.CSharp.dll")
                .Execute(context);
        }

        private static void TargetRunTests(ITaskContext context, string projectName)
        {
            NUnitWithDotCoverTask task = new NUnitWithDotCoverTask(
                context.Properties.Get<string>(BuildProps.NUnitConsolePath),
                Path.Combine(projectName, "bin", context.Properties[BuildProps.BuildConfiguration], projectName) + ".dll");
            task.DotCoverFilters =
                "-:module=*.Tests;-:class=*Contract;-:class=*Contract`*;-:class=JetBrains.Annotations.*;-:module=NUnit*;-:module=Moq*";
            task.FailBuildOnViolations = false;
            task.NUnitCmdLineOptions = "--labels=All --trace=Verbose";
            task.Execute(context);
        }

        private static void TargetFetchBuildVersion(ITaskContext context)
        {
            Version version = BuildTargets.FetchBuildVersionFromFile(context);
            context.Properties.Set(BuildProps.BuildVersion, version);
            context.WriteInfo("The build version will be {0}", version);
        }

        private static void TargetNuGet(ITaskContext context, string nugetId)
        {
            PublishNuGetPackageTask publishTask = new PublishNuGetPackageTask(nugetId, @"Flubu\Flubu.nuspec");
            publishTask.ForApiKeyUseEnvironmentVariable();
            publishTask.Execute(context);
        }
    }
}
