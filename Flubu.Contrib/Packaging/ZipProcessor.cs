using System.Collections.Generic;

namespace Flubu.Packaging
{
    public class ZipProcessor : IPackageProcessor
    {
        public ZipProcessor(
            ITaskContext taskContext,
            IZipper zipper, 
            FileFullPath zipFileName, 
            FullPath baseDir,
            int? compressionLevel,
            params string[] sources)
        {
            this.taskContext = taskContext;
            this.zipper = zipper;
            this.zipFileName = zipFileName;
            this.baseDir = baseDir;
            this.compressionLevel = compressionLevel;
            sourcesToZip.AddRange(sources);
        }

        public IPackageDef Process(IPackageDef packageDef)
        {
            StandardPackageDef zippedPackageDef = new StandardPackageDef();
            List<FileFullPath> filesToZip = new List<FileFullPath>();

            foreach (IFilesSource childSource in packageDef.ListChildSources())
            {
                if (sourcesToZip.Contains(childSource.Id))
                {
                    foreach (PackagedFileInfo file in childSource.ListFiles())
                    {
                        if (false == LoggingHelper.LogIfFilteredOut(file.FileFullPath.ToString(), filter, taskContext))
                            continue;

                        taskContext.WriteDebug("Adding file '{0}' to zip package", file.FileFullPath);
                        filesToZip.Add(file.FileFullPath);
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

        private readonly List<string> sourcesToZip = new List<string>();
        private readonly ITaskContext taskContext;
        private readonly IZipper zipper;
        private readonly FileFullPath zipFileName;
        private readonly FullPath baseDir;
        private readonly int? compressionLevel;
        private IFileFilter filter;
    }
}