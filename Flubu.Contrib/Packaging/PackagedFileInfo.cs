namespace Flubu.Packaging
{
    public class PackagedFileInfo
    {
        public PackagedFileInfo(FullPath fullPath, LocalPath localPath)
        {
            //AssertIsFullPath(fullPath);

            this.localPath = localPath;
            this.fullPath = fullPath;
        }

        public PackagedFileInfo(string fullPath, string localPath)
            : this (new FullPath(fullPath), new LocalPath(localPath))
        {
        }

        public PackagedFileInfo(FullPath fullPath)
        {
            //AssertIsFullPath(fullPath);

            this.fullPath = fullPath;
        }

        public static PackagedFileInfo FromLocalPath (string path)
        {
            return new PackagedFileInfo(new FullPath(path));
        }

        public LocalPath LocalPath
        {
            get { return localPath; }
        }

        public FullPath FullPath
        {
            get { return fullPath; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PackagedFileInfo that = (PackagedFileInfo) obj;

            return string.Equals(localPath, that.localPath) && string.Equals(fullPath, that.fullPath);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //private void AssertIsFullPath(string path)
        //{
        //    if (false == Path.IsPathRooted(path))
        //    {
        //        string message = string.Format(
        //            CultureInfo.InvariantCulture,
        //            "Path '{0}' must be absolute.",
        //            path);
        //        throw new ArgumentException("path", message);
        //    }
        //}

        private LocalPath localPath;
        private FullPath fullPath;
    }
}