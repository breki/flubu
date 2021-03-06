using System;
using System.IO;

namespace Flubu.Tasks.FileSystem
{
    public class DeleteFilesTask : TaskBase
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
                    "Delete files from directory {0} matching pattern '{1}'", 
                    directoryPath, 
                    filePattern);
            }
        }

        public DeleteFilesTask (string directoryPath, string filePattern, bool recursive)
        {
            this.directoryPath = directoryPath;
            this.filePattern = filePattern;
            this.recursive = recursive;
        }

        public static void Execute(
            ITaskContext context, 
            string directoryPath, 
            string filePattern, 
            bool recursive)
        {
            DeleteFilesTask task = new DeleteFilesTask (directoryPath, filePattern, recursive);
            task.Execute (context);
        }

        protected override void DoExecute (ITaskContext context)
        {
            SearchOption searchOption = SearchOption.TopDirectoryOnly;
            if (recursive)
                searchOption = SearchOption.AllDirectories;

            foreach (string file in Directory.EnumerateFiles (directoryPath, filePattern, searchOption))
            {
                File.Delete (file);
                context.WriteInfo("Deleted file '{0}'", file);
            }
        }

        private readonly string directoryPath;
        private readonly string filePattern;
        private readonly bool recursive;
    }
}
