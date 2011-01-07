using System.Globalization;
using System.IO;

namespace Flubu.Builds
{
    public class SimpleBuildProduct<TRunner> : BuildProduct<TRunner>
        where TRunner : FlubuRunner<TRunner>
    {
        public SimpleBuildProduct(
            string productPartId,
            string sourceDirectory, 
            string productDirectoryPath,
            string includePattern,
            string excludePattern) : base(productPartId)
        {
            this.sourceDirectory = sourceDirectory;
            this.productDirectoryPath = productDirectoryPath;
            this.includePattern = includePattern;
            this.excludePattern = excludePattern;
        }

        public override void CopyProductFiles(
            TRunner buildRunner,
            string packageDirectory)
        {
            ClearCopiedFilesList();

            string actualExcludePattern = @"\.svn";

            if (excludePattern != null)
            {
                actualExcludePattern = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}|{1}",
                    actualExcludePattern,
                    excludePattern);
            }

            buildRunner.CopyDirectoryStructure(
                sourceDirectory, 
                Path.Combine(packageDirectory, productDirectoryPath), 
                true,
                includePattern,
                actualExcludePattern);

            AddFilesToCopiedList(buildRunner.LastCopiedFilesList);
        }

        private string excludePattern;
        private string includePattern;
        private string productDirectoryPath;
        private string sourceDirectory;
    }
}