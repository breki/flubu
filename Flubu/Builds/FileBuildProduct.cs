using System.IO;

namespace Flubu.Builds
{
    /// <summary>
    /// Represents a single file as a build product.
    /// </summary>
    /// <typeparam name="TRunner">The type of the runner.</typeparam>
    public class FileBuildProduct<TRunner> : BuildProduct<TRunner> where TRunner : FlubuRunner<TRunner>
    {
        public FileBuildProduct(string productPartId, string sourceFileName, string destinationFileName) : base(productPartId)
        {
            this.sourceFileName = sourceFileName;
            this.destinationFileName = destinationFileName;
        }

        public override void CopyProductFiles(TRunner buildRunner, string packageDirectory)
        {
            ClearCopiedFilesList ();

            string destinationPath = Path.Combine(packageDirectory, destinationFileName);
            AddFileToCopiedList(destinationPath);
            buildRunner.EnsureDirectoryPathExists(destinationPath, true);
            buildRunner.CopyFile(sourceFileName, destinationPath, true);
        }

        private readonly string sourceFileName;
        private readonly string destinationFileName;
    }
}