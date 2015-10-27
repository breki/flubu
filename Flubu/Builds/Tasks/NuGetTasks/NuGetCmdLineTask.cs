using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    public class NuGetCmdLineTask : TaskBase
    {
        public NuGetCmdLineTask(string command, string workingDirectory = null)
        {
            this.command = command;
            this.workingDirectory = workingDirectory;
        }

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Execute NuGet command line tool (command='{0}')", command); }
        }

        public NuGetVerbosity? Verbosity
        {
            get { return verbosity; }
            set { verbosity = value; }
        }

        public string ApiKey { get; set; }

        public int ExitCode
        {
            get { return exitCode; }
        }

        public NuGetCmdLineTask AddArgument (string arg)
        {
            args.Add(arg);
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            string nugetCmdLinePath = FindNuGetCmdLinePath();

            if (nugetCmdLinePath == null)
            {
                context.Fail (
                    "Could not find NuGet.CommandLine package in the {0} directory. You have to download it yourself.", 
                    PackagesDirName);
                return;
            }

            RunProgramTask runProgramTask = new RunProgramTask(nugetCmdLinePath);
            if (workingDirectory != null)
                runProgramTask.SetWorkingDir(workingDirectory);

            runProgramTask.EncloseParametersInQuotes(false);
            runProgramTask.AddArgument(command);

            if (verbosity.HasValue)
                runProgramTask.AddArgument("-Verbosity").AddArgument(verbosity.ToString());
            if (ApiKey != null)
                runProgramTask.AddArgument("-ApiKey").AddSecureArgument(ApiKey);

            foreach (string arg in args)
                runProgramTask.AddArgument(arg);

            runProgramTask.Execute(context);
            exitCode = runProgramTask.LastExitCode;
        }

        private static string FindNuGetCmdLinePath()
        {
            if (!Directory.Exists (PackagesDirName))
                return null;

            const string NuGetCmdLinePackageName = "NuGet.CommandLine";
            int packageNameLen = NuGetCmdLinePackageName.Length;

            string highestVersionDir = null;
            Version highestVersion = null;

            foreach (string directory in Directory.EnumerateDirectories (
                PackagesDirName,
                string.Format(CultureInfo.InvariantCulture, "{0}.*", NuGetCmdLinePackageName)))
            {
                string dirLocalName = Path.GetFileName(directory);
                // ReSharper disable once PossibleNullReferenceException
                string versionStr = dirLocalName.Substring (packageNameLen + 1);

                Version version;
                if (!Version.TryParse (versionStr, out version))
                    continue;

                if (highestVersion == null || version > highestVersion)
                {
                    highestVersion = version;
                    highestVersionDir = directory;
                }
            }

            if (highestVersionDir != null)
                return Path.Combine(highestVersionDir, "tools/NuGet.exe");

            return null;
        }

        private readonly string command;
        private readonly string workingDirectory;
        private const string PackagesDirName = "packages";
        private readonly List<string> args = new List<string>();
        private NuGetVerbosity? verbosity;
        private int exitCode;

        public enum NuGetVerbosity
        {
            Normal,
            Quiet,
            Detailed
        }
    }
}