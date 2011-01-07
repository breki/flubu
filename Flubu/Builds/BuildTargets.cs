using System;
using System.Globalization;
using System.IO;
using Flubu.Builds.Tasks;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Targeting;
using Flubu.Tasks.FileSystem;

namespace Flubu.Builds
{
    public static class BuildTargets
    {
        public static void FillBuildTargets(TargetTree targetTree)
        {
            targetTree.AddTarget("clean.output")
                .SetDescription("Cleans solution outputs")
                .DependsOn("load.solution")
                .Do(TargetCleanOutput);

            targetTree.AddTarget("compile")
                .SetDescription("Compiles the VS solution")
                .DependsOn("prepare.build.dir", "load.solution", "clean.output", "fetch.build.version", "generate.commonassinfo")
                .Do(TargetCompile);

            targetTree.AddTarget("fetch.build.version")
                .SetDescription("Fetches the build version")
                .Do(TargetFetchBuildVersion);

            targetTree.AddTarget("generate.commonassinfo")
                .SetDescription("Generates CommonAssemblyInfo.cs file")
                .Do(TargetGenerateCommonAssemblyInfo);

            targetTree.AddTarget("load.solution")
                .SetDescription("Loads & analyzes VS solution")
                .Do(TargetLoadSolution);

            targetTree.AddTarget("prepare.build.dir")
                .SetDescription("Prepares the build directory")
                .Do(TargetPrepareBuildDir);
        }

        public static void TargetCleanOutput(ITaskContext context)
        {
            //TaskContext.LogTaskStarted("Cleaning solution outputs");

            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            string productRootDir = context.Properties.Get<string>(BuildProps.ProductRootDir, ".");

            VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);

            solution.ForEachProject(
                delegate(VSProjectInfo projectInfo)
                    {
                        if (projectInfo is VSProjectWithFileInfo)
                        {
                            VSProjectWithFileInfo info = (VSProjectWithFileInfo)projectInfo;

                            string projectOutputPath = info.GetProjectOutputPath(buildConfiguration);

                            if (projectOutputPath == null)
                                return;

                            projectOutputPath = Path.Combine(info.ProjectDirectoryPath, projectOutputPath);

                            DeleteDirectoryTask.Execute(context, projectOutputPath, false);

                            string projectObjPath = String.Format(
                                CultureInfo.InvariantCulture,
                                @"{0}\obj\{1}",
                                projectInfo.ProjectName,
                                buildConfiguration);
                            projectObjPath = Path.Combine(productRootDir, projectObjPath);
                            DeleteDirectoryTask.Execute(context, projectObjPath, false);
                        }
                    });

            //TaskContext.LogTaskFinished();
        }

        public static void TargetCompile(ITaskContext context)
        {
            VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);
            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            string targetDotNetVersion = context.Properties.Get<string>(BuildProps.TargetDotNetVersion);

            CompileSolutionTask task = new CompileSolutionTask(
                solution.SolutionFileName,
                buildConfiguration,
                targetDotNetVersion);
            task.Execute(context);
        }

        public static void TargetFetchBuildVersion(ITaskContext context)
        {
            string productRootDir = context.Properties.Get<string>(BuildProps.ProductRootDir, ".");
            string productId = context.Properties.Get<string>(BuildProps.ProductId);

            IFetchBuildVersionTask task = null;
            if (HudsonHelper.IsRunningUnderHudson)
                task = new FetchBuildVersionFromHudsonTask(productRootDir, productId);
            else
                task = new FetchBuildVersionFromFileTask(productRootDir, productId);

            task.Execute(context);

            context.Properties.Set(BuildProps.BuildVersion, task.BuildVersion);
        }

        public static void TargetGenerateCommonAssemblyInfo(ITaskContext context)
        {
            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            Version buildVersion = context.Properties.Get<Version>(BuildProps.BuildVersion);
            string companyCopyright = context.Properties.Get<string>(BuildProps.CompanyCopyright, String.Empty);
            string companyName = context.Properties.Get<string>(BuildProps.CompanyName, String.Empty);
            string companyTrademark = context.Properties.Get(BuildProps.CompanyTrademark, String.Empty);
            string productId = context.Properties.Get<string>(BuildProps.ProductId);
            string productName = context.Properties.Get(BuildProps.ProductName, productId);
            string productRootDir = context.Properties.Get(BuildProps.ProductRootDir, ".");

            GenerateCommonAssemblyInfoTask task = new GenerateCommonAssemblyInfoTask(productRootDir, buildVersion);
            task.BuildConfiguration = buildConfiguration;
            task.CompanyCopyright = companyCopyright;
            task.CompanyName = companyName;
            task.CompanyTrademark = companyTrademark;
            task.GenerateConfigurationAttribute = true;
            task.ProductName = productName;
            task.Execute(context);
        }

        public static void TargetLoadSolution(ITaskContext context)
        {
            string solutionFileName = context.Properties.Get<string>(BuildProps.SolutionFileName);
            VSSolution solution = VSSolution.Load(solutionFileName);
            context.Properties.Set(BuildProps.Solution, solution);

            solution.ForEachProject(delegate(VSProjectInfo projectInfo)
            {
                if (projectInfo.ProjectTypeGuid != VSProjectType.CSharpProjectType.ProjectTypeGuid)
                    return;

                //projectExtendedInfos.Add(
                //    projectInfo.ProjectName,
                //    new VSProjectExtendedInfo(projectInfo));
            });

            // also load project files
            solution.LoadProjects();
        }

        public static void TargetPrepareBuildDir(ITaskContext context)
        {
            string buildDir = context.Properties.Get<string>(BuildProps.BuildDir);
            CreateDirectoryTask createDirectoryTask = new CreateDirectoryTask(buildDir, true);
            createDirectoryTask.Execute(context);
        }
    }
}