using System.Globalization;
using ICSharpCode.SharpZipLib.Zip;

namespace Flubu.Tasks.FileSystem
{
    public class UnzipFilesTask : TaskBase
    {
        public UnzipFilesTask(string zipFileName, string destinationDirectory)
        {
            this.zipFileName = zipFileName;
            this.destinationDirectory = destinationDirectory;
        }

        public override string Description
        {
            get
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Unzipping '{0}' to '{1}'",
                    zipFileName,
                    destinationDirectory); 
    
                return message;
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            FastZip fastZip = new FastZip();
            fastZip.CreateEmptyDirectories = true;
            fastZip.ExtractZip(
                zipFileName,
                destinationDirectory,
                FastZip.Overwrite.Always,
                null,
                null,
                null,
                true);
        }

        private readonly string zipFileName;
        private readonly string destinationDirectory;
    }
}