using System.Collections.Generic;

namespace Flubu.Packaging
{
    public class ZipProcessor : IPackageProcessor
    {
        public ZipProcessor(
            ILogger logger,
            IZipper zipper, 
            string zipFileName, 
            string baseDir,
            int? compressionLevel,
            params string[] sources)
        {
            this.logger = logger;
            this.zipper = zipper;
            this.zipFileName = zipFileName;
            this.baseDir = baseDir;
            this.compressionLevel = compressionLevel;
            sourcesToZip.AddRange(sources);
        }

        public IPackageDef Process(IPackageDef packageDef)
        {
            StandardPackageDef zippedPackageDef = new StandardPackageDef();
            List<string> filesToZip = new List<string>();

            foreach (IFilesSource childSource in packageDef.ListChildSources())
            {
                if (sourcesToZip.Contains(childSource.Id))
                {
                    foreach (PackagedFileInfo file in childSource.ListFiles())
                    {
                        if (false == LoggingHelper.LogIfFilteredOut(file.FullPath.ToString(), filter, logger))
                            continue;

                        logger.Log("Adding file '{0}' to zip package", file.FullPath);
                        filesToZip.Add(file.FullPath.ToString());
                    }
                }
            }

            zipper.ZipFiles(zipFileName, baseDir, compressionLevel, filesToZip);

            SingleFileSource singleFileSource = new SingleFileSource("zip", zipFileName);
            zippedPackageDef.AddFilesSource(singleFileSource);

            return zippedPackageDef;
        }

        public void SetFilter(IFileFilter filter)
        {
            this.filter = filter;
        }

        private List<string> sourcesToZip = new List<string>();
        private readonly ILogger logger;
        private readonly IZipper zipper;
        private readonly string zipFileName;
        private readonly string baseDir;
        private readonly int? compressionLevel;
        private IFileFilter filter;
    }
}