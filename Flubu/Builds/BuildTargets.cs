using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Speech.Synthesis;
using Flubu.Builds.Tasks;
using Flubu.Builds.VSSolutionBrowsing;
using Flubu.Targeting;
using Flubu.Tasks.FileSystem;
using Flubu.Tasks.Tests;

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

            targetTree.AddTarget("before.compile")
                .SetDescription("Steps before compiling the VS solution")
                .DependsOn("prepare.build.dir", "load.solution", "clean.output", "generate.commonassinfo")
                .SetAsHidden();

            targetTree.AddTarget("compile")
                .SetDescription("Compile the VS solution")
                .DependsOn("before.compile")
                .Do(TargetCompile);

            targetTree.AddTarget("fetch.build.version")
                .SetDescription("Fetch the build version")
                .SetAsHidden();

            targetTree.AddTarget("fxcop")
                .SetDescription("Run FxCop")
                .Do(TargetFxcop);

            targetTree.AddTarget("generate.commonassinfo")
                .SetDescription("Generate CommonAssemblyInfo.cs file")
                .DependsOn("fetch.build.version")
                .Do(TargetGenerateCommonAssemblyInfo);

            targetTree.AddTarget("load.solution")
                .SetDescription("Load & analyze VS solution")
                .Do(TargetLoadSolution)
                .SetAsHidden ();

            targetTree.AddTarget("prepare.build.dir")
                .SetDescription("Prepare the build directory")
                .Do(TargetPrepareBuildDir)
                .SetAsHidden ();
        }

        public static void FillDefaultProperties (ITaskContext context)
        {
            context.Properties.Set(BuildProps.BuildConfiguration, "Release");
            context.Properties.Set(BuildProps.BuildDir, "Builds");
            context.Properties.Set(BuildProps.BuildLogsDir, @"Builds\BuildLogs");
            context.Properties.Set(BuildProps.FxcopDir, "Microsoft FxCop 1.36");
            context.Properties.Set(BuildProps.GallioEchoPath, @"lib\Gallio\bin\Gallio.Echo.exe");
            context.Properties.Set(BuildProps.LibDir, "lib");
            context.Properties.Set(BuildProps.ProductRootDir, ".");
            context.Properties.Set(BuildProps.TargetDotNetVersion, FlubuEnvironment.Net35VersionNumber);
        }

        public static Version FetchBuildVersionFromFile (ITaskContext context)
        {
            string productRootDir = context.Properties.Get (BuildProps.ProductRootDir, ".");
            string productId = context.Properties.Get<string> (BuildProps.ProductId);

            IFetchBuildVersionTask task = new FetchBuildVersionFromFileTask (productRootDir, productId);
            task.Execute (context);
            return task.BuildVersion;
        }

        [SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static int FetchBuildNumberFromFile (ITaskContext context)
        {
            string productRootDir = context.Properties.Get (BuildProps.ProductRootDir, ".");
            string productId = context.Properties.Get<string> (BuildProps.ProductId);
            string projectBuildNumberFileName = Path.Combine (productRootDir, productId + ".BuildNumber.txt");

            if (false == File.Exists (projectBuildNumberFileName))
                return 1;

            using (Stream stream = File.Open (projectBuildNumberFileName, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader (stream))
                {
                    string buildNumberAsString = reader.ReadLine ();
                    try
                    {
                        return int.Parse(buildNumberAsString, CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        return 1;
                    }
                }
            }
        }

        public static void IncrementBuildNumberInFile (ITaskContext context)
        {
            string productRootDir = context.Properties.Get (BuildProps.ProductRootDir, ".");
            string productId = context.Properties.Get<string> (BuildProps.ProductId);
            string projectBuildNumberFileName = Path.Combine (productRootDir, productId + ".BuildNumber.txt");

            int nextBuildNumber = context.Properties.Get<Version>(BuildProps.BuildVersion).Build + 1;
            File.WriteAllText(projectBuildNumberFileName, nextBuildNumber.ToString(CultureInfo.InvariantCulture));

            context.WriteInfo("Incrementing the next build number to {0}", nextBuildNumber);
        }

        public static Version FetchBuildVersionFromHudson (ITaskContext context)
        {
            string productRootDir = context.Properties.Get (BuildProps.ProductRootDir, ".");
            string productId = context.Properties.Get<string> (BuildProps.ProductId);

            VersionControlSystem versionControlSystem = context.Properties.Get<VersionControlSystem> (
                BuildProps.VersionControlSystem);

            IFetchBuildVersionTask task = new FetchBuildVersionFromHudsonTask (
                productRootDir,
                productId,
                v =>
                {
                    int hudsonBuildNumber = HudsonHelper.BuildNumber;
                    int revisionNumber;

                    switch (versionControlSystem)
                    {
                        case VersionControlSystem.Subversion:
                            revisionNumber = HudsonHelper.SvnRevision;
                            break;
                        case VersionControlSystem.Mercurial:
                            revisionNumber = 0;
                            break;
                        default:
                            throw new NotSupportedException ();
                    }

                    return new Version (
                        v.Major,
                        v.Minor,
                        revisionNumber,
                        hudsonBuildNumber);
                });

            task.Execute (context);
            return task.BuildVersion;
        }

        public static void OnBuildFinished (ITaskSession session)
        {
            session.ResetDepth();

            LogTargetDurations(session);

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

            if (!HudsonHelper.IsRunningUnderHudson)
            {
                using (SpeechSynthesizer speech = new SpeechSynthesizer())
                {
                    PromptBuilder builder = new PromptBuilder(new CultureInfo("en-US"));
                    builder.StartStyle(new PromptStyle(PromptRate.Slow));
                    builder.StartStyle(new PromptStyle(PromptVolume.Loud));
                    builder.StartSentence(new CultureInfo("en-US"));
                    builder.AppendText("Build " + (session.HasFailed ? "failed." : "successful!"));
                    builder.EndSentence();
                    builder.EndStyle();
                    builder.EndStyle();
                    speech.Speak(builder);
                }
            }

            //Beeper.Beep(session.HasFailed ? MessageBeepType.Error : MessageBeepType.Ok);
        }

        public static void LogTargetDurations(ITaskSession session)
        {
            if (session.TargetTree == null)
                return;

            session.WriteInfo(String.Empty);

            SortedList<string, ITarget> sortedTargets = new SortedList<string, ITarget>();

            foreach (ITarget target in session.TargetTree.EnumerateExecutedTargets())
                sortedTargets.Add(target.TargetName, target);

            foreach (ITarget target in sortedTargets.Values)
            {
                if (target.TaskStopwatch.ElapsedTicks > 0)
                {
                    session.WriteInfo(
                        "Target {0} took {1} s",
                        target.TargetName,
                        (int)target.TaskStopwatch.Elapsed.TotalSeconds);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static void TargetCleanOutput(ITaskContext context)
        {
            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            string productRootDir = context.Properties.Get(BuildProps.ProductRootDir, ".");

            VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);

            solution.ForEachProject(
                delegate(VSProjectInfo projectInfo)
                    {
                        if (projectInfo is VSProjectWithFileInfo)
                        {
                            VSProjectWithFileInfo info = (VSProjectWithFileInfo)projectInfo;

                            LocalPath projectOutputPath = info.GetProjectOutputPath(buildConfiguration);

                            if (projectOutputPath == null)
                                return;

                            FullPath projectFullOutputPath = info.ProjectDirectoryPath.CombineWith(projectOutputPath);
                            DeleteDirectoryTask.Execute(context, projectFullOutputPath.ToString(), false);

                            string projectObjPath = String.Format(
                                CultureInfo.InvariantCulture,
                                @"{0}\obj\{1}",
                                projectInfo.ProjectName,
                                buildConfiguration);
                            projectObjPath = Path.Combine(productRootDir, projectObjPath);
                            DeleteDirectoryTask.Execute(context, projectObjPath, false);
                        }
                    });
        }

        public static void TargetCompile(ITaskContext context)
        {
            VSSolution solution = context.Properties.Get<VSSolution>(BuildProps.Solution);
            string buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            string targetDotNetVersion = context.Properties.Get<string>(BuildProps.TargetDotNetVersion);

            CompileSolutionTask task = new CompileSolutionTask(
                solution.SolutionFileName.ToString(),
                buildConfiguration,
                targetDotNetVersion);
            task.Execute(context);
        }

        public static void TargetFxcop(ITaskContext context)
        {
            FullPath rootDir = new FullPath(context.Properties[BuildProps.ProductRootDir]);

            FullPath fxcopDir = rootDir
                .CombineWith(context.Properties[BuildProps.LibDir])
                .CombineWith(context.Properties[BuildProps.FxcopDir]);

            FullPath buildLogsPath = new FullPath(context.Properties[BuildProps.ProductRootDir])
                .CombineWith(context.Properties[BuildProps.BuildLogsDir]);

            RunFxcopTask task = new RunFxcopTask(
                fxcopDir.AddFileName("FxCopCmd.exe").ToString(),
                fxcopDir.AddFileName("FxCop.exe").ToString(),
                rootDir.AddFileName("{0}.FxCop", context.Properties[BuildProps.ProductId]).ToString(),
                buildLogsPath.AddFileName("{0}.FxCopReport.xml", context.Properties[BuildProps.ProductId]).ToString());
            task.Execute(context);
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
            bool generateAssemblyVersion = context.Properties.Get(BuildProps.AutoAssemblyVersion, true);

            GenerateCommonAssemblyInfoTask task = new GenerateCommonAssemblyInfoTask(productRootDir, buildVersion);
            task.BuildConfiguration = buildConfiguration;
            task.CompanyCopyright = companyCopyright;
            task.CompanyName = companyName;
            task.CompanyTrademark = companyTrademark;
            task.GenerateConfigurationAttribute = true;
            task.ProductName = productName;
            task.GenerateAssemblyVersion = generateAssemblyVersion;
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

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        public static void TargetRunTests(
            ITaskContext context, 
            string projectName, 
            string filter,
            ref int testsRunCounter)
        {
            FullPath buildLogsPath = new FullPath(context.Properties[BuildProps.ProductRootDir])
                .CombineWith(context.Properties[BuildProps.BuildLogsDir]);

            RunGallioTestsTask task = new RunGallioTestsTask(
                projectName,
                context.Properties.Get<VSSolution>(BuildProps.Solution),
                context.Properties.Get<string>(BuildProps.BuildConfiguration),
                context.Properties.Get<string>(BuildProps.GallioEchoPath),
                ref testsRunCounter,
                buildLogsPath.ToString());
            task.Filter = filter;
            task.Execute(context);
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        public static void TargetRunNUnitTests(
            ITaskContext context,
            string projectName,
            string filter,
            ref int testsRunCounter)
        {
            //FullPath buildLogsPath = new FullPath(context.Properties[BuildProps.ProductRootDir])
            //    .CombineWith(context.Properties[BuildProps.BuildLogsDir]);
            var solution = context.Properties.Get<VSSolution>(BuildProps.Solution);
            VSProjectWithFileInfo project = (VSProjectWithFileInfo)solution.FindProjectByName(projectName);
            var buildConfiguration = context.Properties.Get<string>(BuildProps.BuildConfiguration);
            var testFileName =
                project.ProjectDirectoryPath.CombineWith(project.GetProjectOutputPath(buildConfiguration)).
                    AddFileName("{0}.dll", project.ProjectName);

            NUnitTask task = new NUnitTask(Path.GetDirectoryName(testFileName.ToString()), Path.GetFileName(testFileName.ToString()));
            task.Execute(context);
        }
    }
}