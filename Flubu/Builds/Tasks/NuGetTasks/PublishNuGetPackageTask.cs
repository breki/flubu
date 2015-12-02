using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Text;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    public class PublishNuGetPackageTask : TaskBase
    {
        public const string DefaultNuGetApiKeyEnvVariable = "NuGetOrgApiKey";
        public const string DefaultApiKeyFileName = "private/nuget.org-api-key.txt";

        public PublishNuGetPackageTask (string packageId, string nuspecFileName)
        {
            this.packageId = packageId;
            this.nuspecFileName = nuspecFileName;
        }

        public override string Description
        {
            get
            {
                return string.Format (
                    CultureInfo.InvariantCulture,
                    "Push NuGet package {0} to NuGet server",
                    packageId);
            }
        }

        public string BasePath
        {
            get { return basePath; }
            set { basePath = value; }
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

        public void ForApiKeyUse (string apiKey)
        {
            apiKeyFunc = c => apiKey;
        }

        public void ForApiKeyUseEnvironmentVariable (string variableName = DefaultNuGetApiKeyEnvVariable)
        {
            apiKeyFunc = c => FetchNuGetApiKeyFromEnvVariable (c, variableName);
        }

        public void ForApiKeyUseFile (string fileName)
        {
            apiKeyFunc = c => FetchNuGetApiKeyFromLocalFile (c, fileName);
        }

        protected override void DoExecute (ITaskContext context)
        {
            FullPath packagesDir = new FullPath (context.Properties.Get (BuildProps.ProductRootDir, "."));
            packagesDir = packagesDir.CombineWith (context.Properties.Get<string> (BuildProps.BuildDir));

            FileFullPath destNuspecFile = packagesDir.AddFileName ("{0}.nuspec", packageId);

            context.WriteInfo ("Preparing the {0} file", destNuspecFile);
            ReplaceTokensTask task = new ReplaceTokensTask (
                nuspecFileName,
                destNuspecFile.ToString ());
            task.AddTokenValue ("version", context.Properties.Get<Version> (BuildProps.BuildVersion).ToString ());
            task.Execute (context);

            // package it
            context.WriteInfo ("Creating a NuGet package file");
            string nugetWorkingDir = destNuspecFile.Directory.ToString ();
            NuGetCmdLineTask nugetTask = new NuGetCmdLineTask ("pack", nugetWorkingDir);
            nugetTask.Verbosity = NuGetCmdLineTask.NuGetVerbosity.Detailed;
            nugetTask
                .AddArgument (destNuspecFile.FileName);

            if (basePath != null)
                nugetTask.AddArgument ("-BasePath").AddArgument (basePath);

            nugetTask
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

            if (apiKeyFunc == null)
                throw new InvalidOperationException ("NuGet API key was not provided");

            string apiKey = apiKeyFunc (context);
            if (apiKey == null)
                return;

            // publish the package file
            context.WriteInfo ("Pushing the NuGet package to the repository");

            nugetTask = new NuGetCmdLineTask ("push", nugetWorkingDir);
            nugetTask.Verbosity = NuGetCmdLineTask.NuGetVerbosity.Detailed;
            nugetTask.ApiKey = apiKey;
            if (nuGetServerUrl != null)
                nugetTask.AddArgument ("Source").AddArgument (nuGetServerUrl);

            nugetTask
                .AddArgument (nupkgFileName)
                .Execute (context);
        }

        private static string FetchNuGetApiKeyFromLocalFile (ITaskContext context, string fileName = DefaultApiKeyFileName)
        {
            if (!File.Exists (fileName))
            {
                context.Fail ("NuGet API key file ('{0}') does not exist, cannot publish the package.", fileName);
                return null;
            }

            return File.ReadAllText (fileName).Trim ();
        }

        private static string FetchNuGetApiKeyFromEnvVariable (ITaskContext context, string environmentVariableName = DefaultNuGetApiKeyEnvVariable)
        {
            string apiKey = Environment.GetEnvironmentVariable (environmentVariableName);

            if (string.IsNullOrEmpty (apiKey))
            {
                context.Fail ("NuGet API key environment variable ('{0}') does not exist, cannot publish the package.", environmentVariableName);
                return null;
            }

            return apiKey;
        }

        private readonly string packageId;
        private readonly string nuspecFileName;
        private bool allowPushOnInteractiveBuild;
        [SuppressMessage ("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private string nuGetServerUrl;
        private Func<ITaskContext, string> apiKeyFunc;
        private string basePath;
    }
}