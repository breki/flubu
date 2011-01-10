using System;
using System.IO;

namespace Flubu.Packaging
{
    public class Copier : ICopier
    {
        public Copier(ITaskContext taskContext)
        {
            this.taskContext = taskContext;
        }

        public void Copy(FileFullPath sourceFileName, FileFullPath destinationFileName)
        {
            string directoryName = destinationFileName.Directory.ToString();

            if (false == String.IsNullOrEmpty(directoryName))
            {
                if (false == Directory.Exists(directoryName))
                {
                    taskContext.WriteDebug("Creating directory '{0}'", directoryName);
                    Directory.CreateDirectory(directoryName);
                }
            }

            taskContext.WriteDebug("Copying file '{0}' to '{1}'", sourceFileName, destinationFileName);
            File.Copy(sourceFileName.ToString(), destinationFileName.ToString(), true);
        }

        private readonly ITaskContext taskContext;
    }
}