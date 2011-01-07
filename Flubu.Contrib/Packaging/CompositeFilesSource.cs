using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Flubu.Packaging
{
    public class CompositeFilesSource : ICompositeFilesSource
    {
        public CompositeFilesSource(string id)
        {
            Id = id;
        }

        public string Id
        {
            get; set;
        }

        public void AddFilesSource(IFilesSource filesSource)
        {
            filesSources.Add(filesSource.Id, filesSource);
        }

        public ICollection<PackagedFileInfo> ListFiles()
        {
            List<PackagedFileInfo> allFiles = new List<PackagedFileInfo>();

            foreach (IFilesSource filesSource in filesSources.Values)
                allFiles.AddRange(filesSource.ListFiles());

            return allFiles;
        }

        public void SetFilter(IFileFilter filter)
        {
            this.filter = filter;
        }

        public ICollection<IFilesSource> ListChildSources()
        {
            return filesSources.Values;
        }

        private readonly Dictionary<string, IFilesSource> filesSources = new Dictionary<string, IFilesSource>();
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IFileFilter filter;
    }
}