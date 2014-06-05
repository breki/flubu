using System;
using System.Diagnostics;
using System.IO;

namespace Flubu.Tasks.FileSystem
{
    /// <summary>
    /// Retrieves file version information from local file.
    /// </summary>
    public class GetFileVersionTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetFileVersionTask"/> class.
        /// </summary>
        /// <param name="file">File name of the file whose version should be retrieved.</param>
        /// <param name="setting">Name of setting to which file version is stored.</param>
        public GetFileVersionTask(string file, string setting)
        {
            this.file = file;
            this.setting = setting;
        }

        public override string Description
        {
            get { return "Retreiving file version for '" + file + "'."; }
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            FileInfo fileInfo = new FileInfo(file);
            FileVersionInfo version = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
            context.Properties.Set(setting, version.FileVersion);
        }

        private readonly string file;
        private readonly string setting;
    }
}