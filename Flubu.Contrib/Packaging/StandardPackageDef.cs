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

        public StandardPackageDef(string id, ILogger logger, IDirectoryFilesLister directoryFilesLister) : base(id)
        {
            Logger = logger;
            FileLister = directoryFilesLister;
        }

        public StandardPackageDef AddFolderSource(string id, string directoryName, bool recursive)
        {
            DirectorySource source = new DirectorySource(Logger, FileLister, id, directoryName, recursive);
            AddFilesSource(source);
            return this;
        }

        public StandardPackageDef AddFolderSource(string id, string directoryName, bool recursive, IFileFilter filter)
        {
            DirectorySource source = new DirectorySource(Logger, FileLister, id, directoryName, recursive);
            source.SetFilter(filter);
            AddFilesSource(source);
            return this;
        }

        public StandardPackageDef AddWebFolderSource(string id, string directoryName, bool recursive)
        {
            DirectorySource source = new DirectorySource(Logger, FileLister, id, directoryName, recursive);
            source.SetFilter(new NegativeFilter(
                    new RegexFileFilter(@"^.*\.(svc|asax|config|aspx|ascx|css|js|gif|PNG)$")));
            AddFilesSource(source);
            return this;
        }

        private ILogger Logger { get; set; }

        private IDirectoryFilesLister FileLister { get; set; }
    }
}