using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Text;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    [SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
    public class PublishNuGetPackageTask : TaskBase
    {
        [SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
        // ReSharper disable once MemberCanBePrivate.Global
        public const string DefaultNuGetApiKeyEnvVariable = "NuGetOrgApiKey";
        // ReSharper disable once MemberCanBePrivate.Global
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
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "Push NuGet package {0} to NuGet server",
                    packageId);
            }
        }

        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the server URL.
        /// </summary>
        /// <remarks>Starting with NuGet 3.4.2, this is a mandatory parameter unless 
        /// <c>DefaultPushSource</c> config value is set in the NuGet config file.
        /// The default <see cref="NuGetServerUrl"/> value is <c>https://www.nuget.org/api/v2/package</c>.</remarks>
        [SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu"), SuppressMessage ("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string NuGetServerUrl
        {
            get
            {
                return nuGetServerUrl;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidOperationException("NuGetServerUrl cannot be null or empty string.");

                nuGetServerUrl = value;
            }
        }

        public bool AllowPushOnInteractiveBuild { get; set; }

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

        public static string ConstructProductVersionStringUsedForNupkg(Version productVersion)
        {
            int versionComponentsUsed = 4;

            if (productVersion.Revision == 0 || productVersion.Revision == -1)
                versionComponentsUsed = 3;

            return productVersion.ToString(versionComponentsUsed);
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
            nugetTask
                .AddArgument (destNuspecFile.FileName);

            nugetTask.AddVerbosityArgument(NuGetCmdLineTask.NuGetVerbosity.Detailed);

            if (BasePath != null)
                nugetTask.AddArgument ("-BasePath").AddArgument (BasePath);

            nugetTask.Execute (context);

            string nupkgFileName = ConstructNupkgFileName(context);
            context.WriteInfo ("NuGet package file {0} created", nupkgFileName);

            // do not push new packages from a local build
            if (context.IsInteractive && !AllowPushOnInteractiveBuild)
                return;

            if (apiKeyFunc == null)
                throw new InvalidOperationException ("NuGet API key was not provided");

            string apiKey = apiKeyFunc (context);
            if (apiKey == null)
            {
                context.WriteInfo("API key function returned null, skipping pushing of the NuGet package.");
                return;
            }

            // publish the package file
            context.WriteInfo ("Pushing the NuGet package to the repository");

            nugetTask = new NuGetCmdLineTask ("push", nugetWorkingDir);
            nugetTask
                .AddArgument(nupkgFileName)
                .AddArgument(apiKey)
                .AddArgument("-Source").AddArgument(nuGetServerUrl)
                .AddVerbosityArgument(NuGetCmdLineTask.NuGetVerbosity.Detailed);

            nugetTask
                .Execute (context);
        }

        private string ConstructNupkgFileName(ITaskContext context)
        {
            Version productVersion = context.Properties.Get<Version> (BuildProps.BuildVersion);

            string productVersionStringUsedForNupkg =
                ConstructProductVersionStringUsedForNupkg(productVersion);

            return string.Format (
                CultureInfo.InvariantCulture, 
                "{0}.{1}.nupkg", 
                packageId,
                productVersionStringUsedForNupkg);
        }

        private static string FetchNuGetApiKeyFromLocalFile(
            ITaskContext context,
            string fileName = DefaultApiKeyFileName)
        {
            if (!File.Exists (fileName))
            {
                context.Fail(
                    "NuGet API key file ('{0}') does not exist, cannot publish the package.",
                    fileName);
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
        [SuppressMessage ("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        private string nuGetServerUrl = "https://www.nuget.org/api/v2/package";
        private Func<ITaskContext, string> apiKeyFunc;
    }
}