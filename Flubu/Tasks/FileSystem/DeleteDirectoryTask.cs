using System;
using System.IO;

namespace Flubu.Tasks.FileSystem
{
    public class DeleteDirectoryTask : TaskBase
    {
        public DeleteDirectoryTask (string directoryPath, bool failIfNotExists)
        {
            this.directoryPath = directoryPath;
            this.failIfNotExists = failIfNotExists;
        }

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
                    "Delete directory '{0}'", 
                    directoryPath);
            }
        }

        public static void Execute(
            ITaskContext environment,
            string directoryPath,
            bool failIfNotExists)
        {
            DeleteDirectoryTask task = new DeleteDirectoryTask (directoryPath, failIfNotExists);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            if (false == Directory.Exists(directoryPath))
            {
                if (false == failIfNotExists)
                    return;
            }

            Directory.Delete (directoryPath, true);
        }

        private string directoryPath;
        private readonly bool failIfNotExists;
    }
}
