using System;
using System.Globalization;
using Flubu.Tasks.Registry;

namespace Flubu.Tasks.Iis
{
    public class GetLocalIisVersionTask : TaskBase
    {
        public const string IisMajorVersion = "IIS/MajorVersion";
        public const string IisMinorVersion = "IIS/MinorVersion";

        public override string Description
        {
            get { return "Get local IIS version"; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is safe to execute in dry run mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is safe to execute in dry run mode; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSafeToExecuteInDryRun
        {
            get
            {
                return true;
            }
        }

        public static void ExecuteTask(ITaskContext environment)
        {
            GetLocalIisVersionTask task = new GetLocalIisVersionTask();
            task.Execute (environment);
        }

        public static string GetIisVersion(ITaskContext environment, bool failIfNotExist)
        {
            if (!environment.Properties.Has(IisMajorVersion))
                ExecuteTask(environment);

            string major = environment.Properties.Get<string>(IisMajorVersion);

            if (string.IsNullOrEmpty(major))
            {
                const string Msg = "IIS not installed or IIS access denied!";
                if (failIfNotExist)
                    throw new RunnerFailedException(Msg);
                environment.WriteInfo(Msg);

                return "0.0";
            }

            string minor = environment.Properties.Get<string>(IisMinorVersion);
            return major + "." + minor;
        }

        internal static int GetMajorVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return 0;
            string[] split = version.Split('.');
            return split.Length != 2 ? 0 : Convert.ToInt32(split[0], CultureInfo.InvariantCulture);
        }

        //internal static int GetMinorVersion(string version)
        //{
        //    if (string.IsNullOrEmpty(version))
        //        return 0;
        //    string[] split = version.Split('.');
        //    return split.Length != 2 ? 0 : Convert.ToInt32(split[1], CultureInfo.InvariantCulture);
        //}

        protected override void DoExecute (ITaskContext context)
        {
            GetRegistryValueTask innerTask = new GetRegistryValueTask (
                Microsoft.Win32.Registry.LocalMachine,
                @"SOFTWARE\Microsoft\InetStp",
                "MajorVersion",
                IisMajorVersion);
            innerTask.Execute (context);

            innerTask = new GetRegistryValueTask (
                Microsoft.Win32.Registry.LocalMachine,
                @"SOFTWARE\Microsoft\InetStp",
                "MinorVersion",
                IisMinorVersion);
            innerTask.Execute (context);

            context.WriteInfo(
                "Local IIS has version {0}.{1}",
                context.Properties.Get<string>("IIS/MajorVersion"),
                context.Properties.Get<string>("IIS/MinorVersion"));
        }
    }
}
