using System;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;
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
                .SetDescription("Clean solution outputs")
                .DependsOn("load.solution")
                .Do(TargetCleanOutput);

            targetTree.AddTarget("compile")
                .SetDescription("Compile the VS solution")
                .DependsOn("prepare.build.dir", "load.solution", "clean.output", "fetch.build.version", "generate.commonassinfo")
                .Do(TargetCompile);

            targetTree.AddTarget("fetch.build.version")
                .SetDescription("Fetch the build version")
                .Do(TargetFetchBuildVersion);

            targetTree.AddTarget("generate.commonassinfo")
                .SetDescription("Generate CommonAssemblyInfo.cs file")
                .Do(TargetGenerateCommonAssemblyInfo);

            targetTree.AddTarget("load.solution")
                .SetDescription("Load & analyze VS solution")
                .Do(TargetLoadSolution);

            targetTree.AddTarget("prepare.build.dir")
                .SetDescription("Prepare the build directory")
                .Do(TargetPrepareBuildDir);
        }

        public static void OnBuildFinished (ITaskSession session)
        {
            session.ResetDepth();

            //LogTargetDurations(runner);

            session.WriteInfo(String.Empty);

            if (session.HasFailed)
                session.WriteError("BUILD FAILED");
            else
                session.WriteInfo("BUILD SUCCESSFUL");

            TimeSpan buildDuration = session.BuildStopwatch.Elapsed;
            session.WriteInfo("Build finish time: {0:g}", DateTime.Now);
            session.WriteInfo(
                "Build duration: {0:D2}:{1:D2}:{2:D2} ({3:d} seconds)",
                buildDuration.Hours,
                buildDuration.Minutes,
                buildDuration.Seconds,
                (int)buildDuration.TotalSeconds);

            using (System.Speech.Synthesis.SpeechSynthesizer speech = new SpeechSynthesizer())
            {
                PromptBuilder builder = new PromptBuilder();
                builder.StartStyle(new PromptStyle(PromptRate.Slow));
                builder.StartSentence();
                builder.AppendText("Build " + (session.HasFailed ? "failed." : "successful!"));
                builder.EndSentence();
                builder.EndStyle();
                speech.Speak(builder);
            }

            //Beeper.Beep(session.HasFailed ? MessageBeepType.Error : MessageBeepType.Ok);
        }

        //public static void LogTargetDurations(ITaskContext taskContext, ITaskSession taskSession)
        //{
        //    taskContext.WriteInfo(String.Empty);

        //    SortedList<string, ITarget> sortedTargets = new SortedList<string, ITarget>();

        //    foreach (ITarget target in runner.Targets.Values)
        //        sortedTargets.Add(target.TargetName, target);

        //    foreach (ITarget target in sortedTargets.Values)
        //    {
        //        if (target.TargetStopwatch.ElapsedTicks > 0)
        //        {
        //            taskContext.WriteInfo(
        //                "Target {0} took {1} s",
        //                target.TargetName,
        //                (int)target.TargetStopwatch.Elapsed.TotalSeconds);
        //        }
        //    }
        //}

        public static void TargetCleanOutput(ITaskContext context)
        {
            //TaskContext.LogTaskStarted("Cleaning solution outputs");

            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            string productRootDir = context.Properties.Get(BuildProps.ProductRootDir, ".");

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
            string productRootDir = context.Properties.Get(BuildProps.ProductRootDir, ".");
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
            string companyCopyright = context.Properties.Get(BuildProps.CompanyCopyright, String.Empty);
            string companyName = context.Properties.Get(BuildProps.CompanyName, String.Empty);
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