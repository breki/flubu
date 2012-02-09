using System;
using System.IO;
using System.Reflection;

namespace Flubu.Tasks.FileSystem
{
    /// <summary>
    /// Retreives .NET assembly version from .NET assembly.
    /// </summary>
    /// <remarks>
    /// If file does not have file version information, setting value is set to null.
    /// </remarks>
    public class GetAssemblyVersionTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAssemblyVersionTask"/> class.
        /// </summary>
        /// <param name="file">Filename of .NET assembly.</param>
        /// <param name="setting">Name of setting to which assembly version is stored.</param>
        public GetAssemblyVersionTask(string file, string setting)
        {
            this.file = file;
            this.setting = setting;
        }

        public override string Description
        {
            get { return "Retreiving assembly version for '" + file + "'."; }
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            FileInfo fileInfo = new FileInfo(file);
            Assembly assembly = Assembly.LoadFile(fileInfo.FullName);
            context.Properties.Set(setting, assembly.GetName().Version.ToString());
        }

        private readonly string file;
        private readonly string setting;
    }
}