using System;

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
    }
}