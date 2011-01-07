using System;
using System.IO;

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
        /// Gets the .NET version number for .NET 1.0.
        /// </summary>
        /// <value>.NET version number for .NET 1.0.</value>
        public static string Net10VersionNumber
        {
            get { return "v1.0.3705"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 1.1.
        /// </summary>
        /// <value>.NET version number for .NET 1.1.</value>
        public static string Net11VersionNumber
        {
            get { return "v1.1.4322"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 2.0.
        /// </summary>
        /// <value>.NET version number for .NET 2.0.</value>
        public static string Net20VersionNumber
        {
            get { return "v2.0.50727"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 3.0.
        /// </summary>
        /// <value>.NET version number for .NET 3.0.</value>
        public static string Net30VersionNumber
        {
            get { return "v3.0"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 3.0.
        /// </summary>
        /// <value>.NET version number for .NET 3.0.</value>
        public static string Net35VersionNumber
        {
            get { return "v3.5"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 3.0.
        /// </summary>
        /// <value>.NET version number for .NET 3.0.</value>
        public static string Net40VersionNumber
        {
            get { return "v4.0.30319"; }
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
        /// Gets the path to the .NET Framework directory.
        /// </summary>
        /// <param name="dotNetVersion">The version of the .NET (example: "v2.0.50727").</param>
        /// <returns>
        /// The path to the .NET Framework directory.
        /// </returns>
        public static string GetDotNetFWDir(string dotNetVersion)
        {
            string fwRootDir = Path.Combine(SystemRootDir, @"Microsoft.NET\Framework");
            return Path.Combine(fwRootDir, dotNetVersion);
        }        
    }
}