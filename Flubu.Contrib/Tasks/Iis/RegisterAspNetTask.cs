using System;

namespace Flubu.Tasks.Iis
{
    public class RegisterAspNetTask : TaskBase
    {
        public override string Description
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
            throw new NotImplementedException("todo next:");

            //string regIisExePath = Path.Combine (
            //    FlubuEnvironment.GetDotNetFWDir(dotNetVersion),
            //    "aspnet_regiis.exe");

            //RunProgramTask runProgramTask = new RunProgramTask(regIisExePath);
            //runProgramTask
            //    .AddArgument("-s")
            //    .AddArgument("{0}{1}", parentVirtualDirectoryName, virtualDirectoryName);
            //runProgramTask.Execute(context);
        }

        private readonly string virtualDirectoryName;
        private string parentVirtualDirectoryName = @"W3SVC/1/ROOT/";
        private readonly string dotNetVersion;
    }
}
