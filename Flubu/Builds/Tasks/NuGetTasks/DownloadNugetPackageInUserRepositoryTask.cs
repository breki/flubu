using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    public class DownloadNugetPackageInUserRepositoryTask : TaskBase
    {
        public DownloadNugetPackageInUserRepositoryTask (string packageId, Version packageVersion = null)
        {
            this.packageId = packageId;
            this.packageVersion = packageVersion;
        }

        public override string Description
        {
            get
            {
                StringBuilder s = new StringBuilder ();
                s.AppendFormat (CultureInfo.InvariantCulture, "Download NuGet package '{0}'", packageId);

                if (packageVersion != null)
                    s.AppendFormat (CultureInfo.InvariantCulture, " (version {0})", packageVersion);

                return s.ToString ();
            }
        }

        public NuGetCmdLineTask.NuGetVerbosity? Verbosity
        {
            get { return verbosity; }
            set { verbosity = value; }
        }

        public string PackageSource
        {
            get { return packageSource; }
            set { packageSource = value; }
        }

        public string ConfigFile
        {
            get { return configFile; }
            set { configFile = value; }
        }

        public string PackageDirectory
        {
            get { return packageDirectory; }
        }

        public const string FlubuCachePathOverrideEnvVariableName = "FLUBU_CACHE";
        private const string NuGetDirectoryName = "NuGet";

        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
        public static string NuGetPackagesCacheDir
        {
            get
            {
                string overrideValue = Environment.GetEnvironmentVariable(FlubuCachePathOverrideEnvVariableName);
                if (overrideValue == null)
                    return Path.Combine (
                        Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), 
                        Path.Combine(".flubu", NuGetDirectoryName));

                return Path.Combine(overrideValue, NuGetDirectoryName);
            }
        }

        protected override void DoExecute (ITaskContext context)
        {
            FindNuGetPackageInUserRepositoryTask findPackageTask = new FindNuGetPackageInUserRepositoryTask (packageId);
            findPackageTask.Execute (context);

            if (findPackageTask.PackageVersion != null && packageVersion != null
                && findPackageTask.PackageVersion > packageVersion)
            {
                packageDirectory = findPackageTask.PackageDirectory;
                return;
            }

            if (findPackageTask.PackageDirectory != null)
            {
                packageDirectory = findPackageTask.PackageDirectory;
                return;
            }

            NuGetCmdLineTask task = new NuGetCmdLineTask ("install")
                .AddArgument (packageId)
                .AddArgument ("-Source").AddArgument (packageSource)
                .AddArgument ("-NonInteractive")
                .AddArgument ("-OutputDirectory").AddArgument (NuGetPackagesCacheDir);

            if (packageVersion != null)
                task.AddArgument ("-Version").AddArgument (packageVersion.ToString ());

            if (configFile != null)
                task.AddArgument ("-ConfigFile").AddArgument (configFile);

            if (verbosity.HasValue)
                task.AddVerbosityArgument(verbosity.Value);

            task.Execute (context);

            findPackageTask.Execute (context);
            packageDirectory = findPackageTask.PackageDirectory;

            if (packageDirectory == null)
                context.Fail(
                    "Something is wrong, after downloading it the NuGet package '{0}' still could not be found.", 
                    packageId);
            else
                context.WriteInfo ("Package downloaded to '{0}'", packageDirectory);
        }

        private readonly string packageId;
        private readonly Version packageVersion;
        private string packageDirectory;
        private string configFile;
        private NuGetCmdLineTask.NuGetVerbosity? verbosity;
        private string packageSource = "https://www.nuget.org/api/v2/";
    }
}