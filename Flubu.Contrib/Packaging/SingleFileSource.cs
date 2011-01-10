using System.Collections.Generic;

namespace Flubu.Packaging
{
    public class SingleFileSource : IFilesSource
    {
        public SingleFileSource(string id, FileFullPath fileName)
        {
            this.id = id;
            this.fileName = fileName;
        }

        public string Id
        {
            get { return id; }
        }

        public ICollection<PackagedFileInfo> ListFiles()
        {
            List<PackagedFileInfo> files = new List<PackagedFileInfo>();
            files.Add(new PackagedFileInfo(fileName));
            return files;
        }

        public void SetFilter(IFileFilter filter)
        {
        }

        private readonly string id;
        private readonly FileFullPath fileName;
    }
}