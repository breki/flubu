using System;
using System.IO;

namespace Flubu.Packaging
{
    public class Copier : ICopier
    {
        public Copier(ILogger logger)
        {
            this.logger = logger;
        }

        public void Copy(string sourceFileName, string destinationFileName)
        {
            string directoryName = Path.GetDirectoryName(destinationFileName);

            if (false == String.IsNullOrEmpty(directoryName))
            {
                if (false == Directory.Exists(directoryName))
                {
                    logger.Log("Creating directory '{0}'", directoryName);
                    Directory.CreateDirectory(directoryName);
                }
            }

            logger.Log("Copying file '{0}' to '{1}'", sourceFileName, destinationFileName);
            File.Copy(sourceFileName, destinationFileName, true);
        }

        private readonly ILogger logger;
    }
}