using System;
using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Tasks.Iis
{
    public class RegisterAspNetTask : TaskBase
    {
        public override string TaskDescription
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Register IIS virtual directory '{0}' for ASP.NET '{1}'", 
                    virtualDirectoryName, 
                    dotNetVersion);
            }
        }

        public RegisterAspNetTask (
            string virtualDirectoryName, 
            string parentVirtualDirectoryName,
            string dotNetVersion) 
            : this (virtualDirectoryName, dotNetVersion)
        {
            this.parentVirtualDirectoryName = parentVirtualDirectoryName;
        }

        public RegisterAspNetTask (string virtualDirectoryName, string dotNetVersion)
        {
            this.virtualDirectoryName = virtualDirectoryName;
            this.dotNetVersion = dotNetVersion;
        }

        public static void Execute(
            ITaskContext environment,
            string virtualDirectoryName,
            string parentVirtualDirectoryName,
            string dotNetVersion)
        {
            RegisterAspNetTask task = new RegisterAspNetTask (virtualDirectoryName, parentVirtualDirectoryName, dotNetVersion);
            task.Execute (environment);
        }

        public static void Execute(
            ITaskContext environment,
            string virtualDirectoryName,
            string dotNetVersion)
        {
            RegisterAspNetTask task = new RegisterAspNetTask (virtualDirectoryName, dotNetVersion);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            string regIisExePath = Path.Combine (
                context.GetDotNetFWDir (dotNetVersion),
                "aspnet_regiis.exe");

            string commandLine = String.Format (
                System.Globalization.CultureInfo.InvariantCulture,
                "-s {0}{1}", 
                parentVirtualDirectoryName, 
                virtualDirectoryName);
            RunProgramTask.Execute (
                context, 
                regIisExePath,
                commandLine, 
                TimeSpan.FromSeconds (30));
        }

        private string virtualDirectoryName;
        private string parentVirtualDirectoryName = @"W3SVC/1/ROOT/";
        private string dotNetVersion;
    }
}
