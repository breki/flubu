using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    public class NuGetCmdLineTask : TaskBase
    {
        public NuGetCmdLineTask(string command)
        {
            this.command = command;
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
            runProgramTask.EncloseParametersInQuotes(false);
            runProgramTask.AddArgument(command);
            foreach (string arg in args)
                runProgramTask.AddArgument(arg);

            runProgramTask.Execute(context);
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
        private const string PackagesDirName = "packages";
        private List<string> args = new List<string>();
        private NuGetVerbosity? verbosity;

        public enum NuGetVerbosity
        {
            Normal,
            Quiet,
            Detailed
        }
    }
}