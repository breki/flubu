using System.Collections.Generic;

namespace Flubu.Packaging
{
    public interface IZipper
    {
        void ZipFiles(
            FileFullPath zipFileName,
            FullPath baseDir,
            int? compressionLevel,
            IEnumerable<FileFullPath> filesToZip);
    }
}