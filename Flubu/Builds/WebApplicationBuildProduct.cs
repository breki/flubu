using System.IO;

namespace Flubu.Builds
{
    public class WebApplicationBuildProduct<TRunner> : BuildProduct<TRunner>
        where TRunner : FlubuRunner<TRunner>
    {
        public WebApplicationBuildProduct(
            string productPartId,
            string sourceDirectory, 
            string productDirectoryName) : base (productPartId)
        {
            this.productDirectoryName = productDirectoryName;
            this.sourceDirectory = sourceDirectory;
        }

        public override void CopyProductFiles(
            TRunner buildRunner,
            string packageDirectory)
        {
            ClearCopiedFilesList();

            buildRunner.CopyDirectoryStructure(
                sourceDirectory,
                Path.Combine(packageDirectory, productDirectoryName),
                true,
                null,
                //@"^.*\.(asax|aspx|ascx|asmx|master|css|gif)$",
                @"obj\\|\.svn|\.(cs|snk|csproj|user|svclog)$|logs\\");

            AddFilesToCopiedList(buildRunner.LastCopiedFilesList);
        }

        private string productDirectoryName;
        private string sourceDirectory;
    }
}