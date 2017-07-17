using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace Flubu
{
    public static class FlubuEnvironment
    {
        /// <summary>
        /// Gets a value indicating whether the script is running on Windows Server 2003.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the script is running on Windows Server 2003; otherwise, <c>false</c>.
        /// </value>
        public static bool IsWinServer2003
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT
                       && Environment.OSVersion.Version.Major == 5
                       && Environment.OSVersion.Version.Minor == 2;
            }
        }

        /// <summary>
        /// Gets the Windows system root directory path.
        /// </summary>
        /// <value>The Windows system root directory path.</value>
        public static string SystemRootDir
        {
            get { return Environment.GetEnvironmentVariable("SystemRoot"); }
        }

        /// <summary>
        /// Returns a sorted dictionary of all MSBuild tools versions that are available on the system.
        /// </summary>
        /// <remarks>The method scans through the registry (<c>HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions</c> path)
        /// to find the available tools versions.</remarks>
        /// <returns>A sorted dictionary whose keys are tools versions (2.0, 3.5, 4.0, 12.0 etc.) and values are paths to the
        /// tools directories (and NOT the <c>MSBuild.exe</c> itself!). The entries are sorted ascendingly by version numbers.</returns>
        [NotNull]
        // ReSharper disable once InconsistentNaming
        public static IDictionary<Version, string> ListAvailableMSBuildToolsVersions ()
        {
            SortedDictionary<Version, string> toolsVersions = new SortedDictionary<Version, string> ();
            FillVersionsFromMSBuildToolsVersionsRegPath(toolsVersions);
            FillVersion15FromVisualStudio2017RegPath(toolsVersions);

            return toolsVersions;
        }

        // ReSharper disable once InconsistentNaming
        private static void FillVersionsFromMSBuildToolsVersionsRegPath(
            SortedDictionary<Version, string> toolsVersions)
        {
            using (RegistryKey toolsVersionsKey =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\MSBuild\ToolsVersions", false))
            {
                if (toolsVersionsKey == null)
                    return;

                foreach (string toolsVersion in toolsVersionsKey.GetSubKeyNames())
                {
                    using (RegistryKey toolsVersionKey = toolsVersionsKey.OpenSubKey(toolsVersion, false))
                    {
                        if (toolsVersionKey == null) continue;

                        object msBuildToolsPathObj = toolsVersionKey.GetValue("MSBuildToolsPath");
                        string msBuildToolsPath = msBuildToolsPathObj as string;
                        if (msBuildToolsPath != null) toolsVersions.Add(new Version(toolsVersion), msBuildToolsPath);
                    }
                }
            }
        }

        private static void FillVersion15FromVisualStudio2017RegPath(SortedDictionary<Version, string> toolsVersions)
        {
            using (RegistryKey vs2017Key =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7", false))
            {
                if (vs2017Key == null)
                    return;

                object key150Value = vs2017Key.GetValue("15.0");
                string vs2017RootPath = key150Value as string;
                if (vs2017RootPath == null)
                    return;

                string msBuildToolsPath = Path.Combine(vs2017RootPath, @"MSBuild\15.0\Bin");
                toolsVersions.Add(new Version("15.0"), msBuildToolsPath);
            }
        }
    }
}