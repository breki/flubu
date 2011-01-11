using System.Collections.Generic;

namespace Flubu.Packaging
{
    public class DirectorySource : IFilesSource
    {
        public DirectorySource(
            ITaskContext taskContext,
            IDirectoryFilesLister directoryFilesLister, 
            string id, 
            FullPath directoryName) : this (taskContext, directoryFilesLister, id, directoryName, true)
        {
            this.taskContext = taskContext;
        }

        public DirectorySource(
            ITaskContext taskContext,
            IDirectoryFilesLister directoryFilesLister,
            string id,
            FullPath directoryName,
            bool recursive)
        {
            this.taskContext = taskContext;
            this.directoryFilesLister = directoryFilesLister;
            this.id = id;
            this.recursive = recursive;
            this.directoryPath = directoryName;
        }

        public string Id
        {
            get { return id; }
        }

        public ICollection<PackagedFileInfo> ListFiles()
        {
            List<PackagedFileInfo> files = new List<PackagedFileInfo>();

            foreach (string fileName in directoryFilesLister.ListFiles(
                directoryPath.ToString(), 
                recursive))
            {
                FileFullPath fileNameFullPath = new FileFullPath(fileName);
                LocalPath debasedFileName = fileNameFullPath.ToFullPath().DebasePath(directoryPath);

                if (false == LoggingHelper.LogIfFilteredOut(fileName, Filter, taskContext))
                    continue;

                PackagedFileInfo packagedFileInfo = new PackagedFileInfo(fileNameFullPath, debasedFileName);
                files.Add(packagedFileInfo);
            }

            return files;
        }

        public void SetFilter(IFileFilter filter)
        {
            Filter = filter;
        }

        public static DirectorySource NoFilterSource(
            ITaskContext taskContext,
            IDirectoryFilesLister directoryFilesLister,
            string id,
            FullPath directoryName,
            bool recursive)
        {
            return new DirectorySource(taskContext, directoryFilesLister, id, directoryName, recursive);
        }

        public static DirectorySource WebFilterSource(
            ITaskContext taskContext,
            IDirectoryFilesLister directoryFilesLister,
            string id,
            FullPath directoryName,
            bool recursive)
        {
            DirectorySource source = new DirectorySource(taskContext, directoryFilesLister, id, directoryName, recursive);
            source.SetFilter(new NegativeFilter(
                    new RegexFileFilter(@"^.*\.(svc|asax|config|aspx|ascx|css|js|gif|PNG)$")));

            return source;
        }

        private IFileFilter Filter { get; set; }

        private readonly ITaskContext taskContext;
        private readonly IDirectoryFilesLister directoryFilesLister;
        private readonly string id;
        private readonly FullPath directoryPath;
        private readonly bool recursive = true;
    }
}