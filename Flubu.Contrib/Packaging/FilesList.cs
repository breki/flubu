using System;
using System.Collections.Generic;

namespace Flubu.Packaging
{
    public class FilesList : IFilesSource
    {
        public FilesList(string id)
        {
            this.id = id;
        }

        public string Id
        {
            get { return id; }
        }

        public void AddFile (PackagedFileInfo packagedFileInfo)
        {
            files.Add(packagedFileInfo);
        }

        public ICollection<PackagedFileInfo> ListFiles()
        {
            if (filter != null)
                return files.FindAll(x => filter.IsPassedThrough(x.FileFullPath.ToString()));

            return files;
        }

        public void SetFilter(IFileFilter filter)
        {
            this.filter = filter;
        }

        private readonly string id;
        private List<PackagedFileInfo> files = new List<PackagedFileInfo>();
        private IFileFilter filter;
    }
}