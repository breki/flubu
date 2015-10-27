using System;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Text;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    public class PublishNuGetPackageTask : TaskBase
    {
        public PublishNuGetPackageTask(string packageId)
        {
            this.packageId = packageId;
        }

        public override string Description
        {
            get
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Push NuGet package {0} to NuGet server",
                    packageId);
            }
        }

        public string NuGetServerUrl
        {
            get { return nuGetServerUrl; }
            set { nuGetServerUrl = value; }
        }

        public bool AllowPushOnInteractiveBuild
        {
            get { return allowPushOnInteractiveBuild; }
            set { allowPushOnInteractiveBuild = value; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            FullPath packagesDir = new FullPath (context.Properties.Get (BuildProps.ProductRootDir, "."));
            packagesDir = packagesDir.CombineWith (context.Properties.Get<string> (BuildProps.BuildDir));

            string sourceNuspecFile = string.Format (
                CultureInfo.InvariantCulture,
                @"{0}\{0}.nuspec",
                packageId);
            FileFullPath destNuspecFile = packagesDir.AddFileName ("{0}.nuspec", packageId);

            context.WriteInfo ("Preparing the {0} file", destNuspecFile);
            ReplaceTokensTask task = new ReplaceTokensTask(
                sourceNuspecFile,
                destNuspecFile.ToString ());
            task.AddTokenValue("version", context.Properties.Get<Version> (BuildProps.BuildVersion).ToString ());
            task.Execute (context);

            // package it
            context.WriteInfo ("Creating a NuGet package file");
            string nugetWorkingDir = destNuspecFile.Directory.ToString ();
            NuGetCmdLineTask nugetTask = new NuGetCmdLineTask ("pack", nugetWorkingDir);
            nugetTask.Verbosity = NuGetCmdLineTask.NuGetVerbosity.Detailed;
            nugetTask
                .AddArgument (destNuspecFile.FileName)
                .Execute (context);

            string nupkgFileName = string.Format (
                CultureInfo.InvariantCulture,
                "{0}.{1}.nupkg",
                packageId,
                context.Properties.Get<Version> (BuildProps.BuildVersion));
            context.WriteInfo ("NuGet package file {0} created", nupkgFileName);

            // do not push new packages from a local build
            if (context.IsInteractive && !allowPushOnInteractiveBuild)
                return;

            string apiKey = FetchNuGetApiKeyFromEnvVariable (context);
            if (apiKey == null)
                return;

            // publish the package file
            context.WriteInfo ("Pushing the NuGet package to the repository");

            nugetTask = new NuGetCmdLineTask ("push", nugetWorkingDir);
            nugetTask.Verbosity = NuGetCmdLineTask.NuGetVerbosity.Detailed;
            nugetTask.ApiKey = apiKey;
            if (nuGetServerUrl != null)
                nugetTask.AddArgument("Source").AddArgument(nuGetServerUrl);

            nugetTask
                .AddArgument (nupkgFileName)
                .Execute (context);
        }

        private static string FetchNuGetApiKeyFromLocalFile (ITaskContext context)
        {
            const string NuGetApiKeyFileName = "private/nuget.org-api-key.txt";
            if (!File.Exists (NuGetApiKeyFileName))
            {
                context.Fail ("NuGet API key file ('{0}') does not exist, cannot publish the package.", NuGetApiKeyFileName);
                return null;
            }

            return File.ReadAllText (NuGetApiKeyFileName);
        }

        private static string FetchNuGetApiKeyFromEnvVariable (ITaskContext context)
        {
            const string NuGetApiKeyEnvVariable = "NuGetOrgApiKey";

            string apiKey = Environment.GetEnvironmentVariable (NuGetApiKeyEnvVariable);

            if (string.IsNullOrEmpty (apiKey))
            {
                context.Fail ("NuGet API key environment variable ('{0}') does not exist, cannot publish the package.", NuGetApiKeyEnvVariable);
                return null;
            }

            return apiKey;
        }

        private readonly string packageId;
        private bool allowPushOnInteractiveBuild;
        private string nuGetServerUrl;
    }
}