using System.Collections.Generic;

namespace Flubu.Packaging
{
    public interface IZipper
    {
        void ZipFiles(
            string zipFileName,
            string baseDir,
            int? compressionLevel,
            IEnumerable<string> filesToZip);
    }
}