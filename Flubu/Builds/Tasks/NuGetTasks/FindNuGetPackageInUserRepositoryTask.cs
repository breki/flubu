﻿using System;
using System.Globalization;
using System.IO;

namespace Flubu.Builds.Tasks.NuGetTasks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Nu")]
    public class FindNuGetPackageInUserRepositoryTask : TaskBase
    {
        public FindNuGetPackageInUserRepositoryTask (string packageId)
        {
            this.packageId = packageId;
        }

        public override string Description
        {
            get
            {
                return string.Format (
                    CultureInfo.InvariantCulture, 
                    "Find NuGet package {0} in user repository", 
                    packageId);
            }
        }

        public string PackageId
        {
            get { return packageId; }
        }

        public Version PackageVersion
        {
            get { return packageVersion; }
        }

        public string PackageDirectory
        {
            get { return packageDirectory; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            packageVersion = null;
            packageDirectory = null;

            if (!Directory.Exists(DownloadNugetPackageInUserRepositoryTask.NuGetPackagesCacheDir))
            {
                context.WriteDebug(
                    "Flubu NuGet user repository directory '{0}' does not exist.", 
                    DownloadNugetPackageInUserRepositoryTask.NuGetPackagesCacheDir);
                return;
            }

            foreach (string directory in Directory.EnumerateDirectories (
                DownloadNugetPackageInUserRepositoryTask.NuGetPackagesCacheDir, 
                string.Format (CultureInfo.InvariantCulture, "{0}.*", packageId)))
            {
                string localDirName = Path.GetFileName (directory);
                // ReSharper disable once PossibleNullReferenceException
                string versionStr = localDirName.Substring (packageId.Length + 1);

                Version version;
                if (!Version.TryParse (versionStr, out version))
                    continue;

                if (packageVersion == null || version > packageVersion)
                {
                    packageVersion = version;
                    packageDirectory = Path.Combine (
                        DownloadNugetPackageInUserRepositoryTask.NuGetPackagesCacheDir, 
                        localDirName);
                }
            }

            if (packageVersion != null)
                context.WriteDebug (
                    "Found NuGet package {0} version {1} in user repository ('{2}')", 
                    packageId, 
                    packageVersion, 
                    packageDirectory);
            else
                context.WriteDebug (
                    "No NuGet package {0} in user repository (should be at '{1}')", 
                    packageId, 
                    packageDirectory);
        }

        private readonly string packageId;
        private Version packageVersion;
        private string packageDirectory;
    }
}