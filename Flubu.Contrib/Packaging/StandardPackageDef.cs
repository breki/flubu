namespace Flubu.Packaging
{
    public class StandardPackageDef : CompositeFilesSource, IPackageDef
    {
        public StandardPackageDef() : base(string.Empty)
        {
        }

        public StandardPackageDef(string id) : base(id)
        {
        }

        public StandardPackageDef(string id, ITaskContext taskContext)
            : base(id)
        {
            this.taskContext = taskContext;
        }

        public StandardPackageDef(string id, ITaskContext taskContext, IDirectoryFilesLister directoryFilesLister) 
            : base(id)
        {
            this.taskContext = taskContext;
            this.fileLister = directoryFilesLister;
        }

        public StandardPackageDef AddFolderSource(string id, FullPath directoryName, bool recursive)
        {
            DirectorySource source = new DirectorySource(taskContext, fileLister, id, directoryName, recursive);
            AddFilesSource(source);
            return this;
        }

        public StandardPackageDef AddFolderSource(string id, FullPath directoryName, bool recursive, IFileFilter filter)
        {
            DirectorySource source = new DirectorySource(taskContext, fileLister, id, directoryName, recursive);
            source.SetFilter(filter);
            AddFilesSource(source);
            return this;
        }

        public StandardPackageDef AddWebFolderSource(string id, FullPath directoryName, bool recursive)
        {
            DirectorySource source = new DirectorySource(taskContext, fileLister, id, directoryName, recursive);
            source.SetFilter(new NegativeFilter(
                    new RegexFileFilter(@"^.*\.(svc|asax|config|aspx|ascx|css|js|gif|PNG|Master)$")));
            AddFilesSource(source);
            return this;
        }

        private readonly ITaskContext taskContext;
        private readonly IDirectoryFilesLister fileLister = new DirectoryFilesLister();
    }
}