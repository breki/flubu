using System;
using System.Collections.Generic;
using System.IO;

namespace Flubu
{
    [Obsolete]
    public abstract class TaskContextBase : ITaskContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether to dry-run the script. 
        /// When set to <c>true</c>, the tasks are not really executed,
        /// instead just a log of activities is created.
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the execution environment runs in an interactive mode.
        /// </summary>
        /// <remarks>When the execution environment is running in the interactive mode, the user has
        /// the ability to influence certain aspects of the environment. One example is entering values
        /// for missing configuration settings.</remarks>
        /// <value><c>true</c> if the execution environment runs in an interactive mode; otherwise, <c>false</c>.</value>
        public bool InteractiveMode { get; set; }

        /// <summary>
        /// Gets a value indicating whether the script is running on Windows Server 2003.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the script is running on Windows Server 2003; otherwise, <c>false</c>.
        /// </value>
        public bool IsWinServer2003
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
        public string Net10VersionNumber
        {
            get { return "v1.0.3705"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 1.1.
        /// </summary>
        /// <value>.NET version number for .NET 1.1.</value>
        public string Net11VersionNumber
        {
            get { return "v1.1.4322"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 2.0.
        /// </summary>
        /// <value>.NET version number for .NET 2.0.</value>
        public string Net20VersionNumber
        {
            get { return "v2.0.50727"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 3.0.
        /// </summary>
        /// <value>.NET version number for .NET 3.0.</value>
        public string Net30VersionNumber
        {
            get { return "v3.0"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 3.0.
        /// </summary>
        /// <value>.NET version number for .NET 3.0.</value>
        public string Net35VersionNumber
        {
            get { return "v3.5"; }
        }

        /// <summary>
        /// Gets the .NET version number for .NET 3.0.
        /// </summary>
        /// <value>.NET version number for .NET 3.0.</value>
        public string Net40VersionNumber
        {
            get { return "v4.0.30319"; }
        }

        /// <summary>
        /// Gets or sets the name of the script.
        /// </summary>
        /// <value>The name of the script.</value>
        public string ScriptName { get; set; }

        /// <summary>
        /// Gets the Windows system root directory path.
        /// </summary>
        /// <value>The Windows system root directory path.</value>
        public string SystemRootDir
        {
            get { return Environment.GetEnvironmentVariable ("SystemRoot"); }
        }

        /// <summary>
        /// Gets the path to the .NET Framework directory.
        /// </summary>
        /// <param name="dotNetVersion">The version of the .NET (example: "v2.0.50727").</param>
        /// <returns>
        /// The path to the .NET Framework directory.
        /// </returns>
        public string GetDotNetFWDir (string dotNetVersion)
        {
            string fwRootDir = Path.Combine (SystemRootDir, @"Microsoft.NET\Framework");
            return Path.Combine (fwRootDir, dotNetVersion);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose ()
        {
            Dispose (true);
            GC.SuppressFinalize (this);
        }

        public void AddLogger(IFlubuLogger logger)
        {
            loggers.Add(logger);
        }

        public abstract string GetConfigSetting (string settingName);

        public abstract bool IsConfigSettingDefined(string settingName);

        public abstract IEnumerable<KeyValuePair<string, string>> ListConfigSettings();

        public void LogError(string message)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogError(message);
        }

        public void LogError(string format, params object[] args)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogError(format, args);
        }

        public void LogMessage(string message)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogMessage(message);
        }

        public void LogMessage(string format, params object[] args)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogMessage(format, args);
        }

        public void LogRunnerFinished(IFlubuRunner runner)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogRunnerFinished(runner);
        }

        public void LogTargetFinished(IFlubuRunnerTarget target)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogTargetFinished(target);
        }

        public void LogTargetStarted(IFlubuRunnerTarget target)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogTargetStarted(target);
        }

        public void LogTaskFinished()
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogTaskFinished();
        }

        public void LogTaskStarted(string taskDescription)
        {
            foreach (IFlubuLogger logger in loggers)
                logger.LogTaskStarted(taskDescription);
        }

        public abstract string ReceiveInput (string prompt);

        public abstract void SetConfigSetting (string settingName, string settingValue);

        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">If <code>false</code>, cleans up native resources. 
        /// If <code>true</code> cleans up both managed and native resources</param>
        protected virtual void Dispose (bool disposing)
        {
            if (false == disposed)
            {
                // TODO: clean native resources         

                if (disposing)
                {
                    foreach (IFlubuLogger logger in loggers)
                        logger.Dispose();
                }

                disposed = true;
            }
        }

        private bool disposed;
        private readonly List<IFlubuLogger> loggers = new List<IFlubuLogger>();
    }
}
