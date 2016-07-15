using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
    public class NuGetCmdLineTask : TaskBase
    {
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
        public const string DefaultNuGetApiKeyEnvVariable = "NuGetOrgApiKey";
        public const string DefaultApiKeyFileName = "private/nuget.org-api-key.txt";

        public NuGetCmdLineTask(string command, string workingDirectory = null)
        {
            this.command = command;
            this.workingDirectory = workingDirectory;
        }

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Execute NuGet command line tool (command='{0}')", command); }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
        public string NuGetCmdLineExePath { get; set; }

        public int ExitCode
        {
            get { return exitCode; }
        }

        public NuGetCmdLineTask AddArgument(string arg)
        {
            args.Add(arg);
            return this;
        }

        public NuGetCmdLineTask AddVerbosityArgument(NuGetVerbosity value)
        {
            args.Add("-Verbosity");
            args.Add(value.ToString());
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (NuGetCmdLineExePath == null)
            {
                Version nuGetCmdLineVersion = FindNuGetCmdLineExePath();

                if (NuGetCmdLineExePath == null)
                {
                    context.Fail(
                        "Could not find NuGet.CommandLine package in the {0} directory. You have to download it yourself.",
                        PackagesDirName);
                    return;
                }

                if (nuGetCmdLineVersion < new Version("3.4.3"))
                {
                    context.Fail(
                        "This version of Flubu NuGet task work with NuGet 3.4.3 or later, but you have an older version {0}. Please download the newer version yourself.",
                        nuGetCmdLineVersion);
                    return;
                }
            }

            RunProgramTask runProgramTask = new RunProgramTask(NuGetCmdLineExePath);
            if (workingDirectory != null)
                runProgramTask.SetWorkingDir(workingDirectory);

            runProgramTask.EncloseParametersInQuotes(false);
            runProgramTask.AddArgument(command);

            foreach (string arg in args)
                runProgramTask.AddArgument(arg);

            runProgramTask.Execute(context);
            exitCode = runProgramTask.LastExitCode;
        }

        private Version FindNuGetCmdLineExePath()
        {
            if (!Directory.Exists(PackagesDirName))
                return null;

            const string NuGetCmdLinePackageName = "NuGet.CommandLine";
            int packageNameLen = NuGetCmdLinePackageName.Length;

            string highestVersionDir = null;
            Version highestVersion = null;

            foreach (string directory in Directory.EnumerateDirectories(
                PackagesDirName,
                string.Format(CultureInfo.InvariantCulture, "{0}.*", NuGetCmdLinePackageName)))
            {
                string dirLocalName = Path.GetFileName(directory);

                // ReSharper disable once PossibleNullReferenceException
                string versionStr = dirLocalName.Substring(packageNameLen + 1);

                Version version;
                if (!Version.TryParse(versionStr, out version))
                    continue;

                if (highestVersion == null || version > highestVersion)
                {
                    highestVersion = version;
                    highestVersionDir = directory;
                }
            }

            if (highestVersionDir != null)
            {
                NuGetCmdLineExePath = Path.Combine(highestVersionDir, "tools/NuGet.exe");
                return highestVersion;
            }

            return null;
        }

        private readonly string command;
        private readonly string workingDirectory;
        private const string PackagesDirName = "packages";
        private readonly List<string> args = new List<string>();
        private int exitCode;

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
        public enum NuGetVerbosity
        {
            Normal,
            Quiet,
            Detailed
        }
    }
}