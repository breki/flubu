using System;
using System.IO;

namespace Flubu.Tasks.FileSystem
{
    public class CopyFileTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture, 
                    "Copy file '{0}' to '{1}'", 
                    sourceFileName, 
                    destinationFileName);
            }
        }

        public CopyFileTask (
            string sourceFileName, 
            string destinationFileName, 
            bool overwrite)
        {
            this.sourceFileName = sourceFileName;
            this.destinationFileName = destinationFileName;
            this.overwrite = overwrite;
        }

        public static void Execute(
            ITaskContext context, 
            string sourceFileName, 
            string destinationFileName, 
            bool overwrite)
        {
            CopyFileTask task = new CopyFileTask (sourceFileName, destinationFileName, overwrite);
            task.Execute (context);
        }

        protected override void DoExecute (ITaskContext context)
        {
            string dir = Path.GetDirectoryName(destinationFileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.Copy (sourceFileName, destinationFileName, overwrite);
        }

        private readonly string sourceFileName;
        private readonly string destinationFileName;
        private readonly bool overwrite;
    }
}
