using System;
using System.IO;

namespace Flubu.Tasks.FileSystem
{
    public class CreateDirectoryTask : TaskBase
    {
        public string DirectoryPath
        {
            get { return directoryPath; }
            set { directoryPath = value; }
        }

        public override string Description
        {
            get
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Create directory '{0}'", 
                    directoryPath);
            }
        }

        public CreateDirectoryTask (string directoryPath, bool forceRecreate)
        {
            this.directoryPath = directoryPath;
            this.forceRecreate = forceRecreate;
        }

        public static void Execute(ITaskContext context, string directoryPath, bool forceRecreate)
        {
            CreateDirectoryTask task = new CreateDirectoryTask(directoryPath, forceRecreate);
            task.Execute(context);
        }

        protected override void DoExecute (ITaskContext context)
        {
            if (Directory.Exists(directoryPath))
            {
                if (forceRecreate)
                    Directory.Delete(directoryPath, true);
                else
                    return;
            }

            Directory.CreateDirectory (directoryPath);
        }

        private string directoryPath;
        private readonly bool forceRecreate;
    }
}
