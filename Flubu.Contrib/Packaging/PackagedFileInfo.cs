namespace Flubu.Packaging
{
    public class PackagedFileInfo
    {
        public PackagedFileInfo(FileFullPath fileFullPath, LocalPath localPath)
        {
            //AssertIsFullPath(fullPath);

            this.localPath = localPath;
            this.fileFullPath = fileFullPath;
        }

        public PackagedFileInfo(string fullPath, string localPath)
            : this(new FileFullPath(fullPath), new LocalPath(localPath))
        {
        }

        public PackagedFileInfo(FileFullPath fileFullPath)
        {
            //AssertIsFullPath(fullPath);

            this.fileFullPath = fileFullPath;
        }

        public static PackagedFileInfo FromLocalPath (string path)
        {
            return new PackagedFileInfo(new FileFullPath(path));
        }

        public LocalPath LocalPath
        {
            get { return localPath; }
        }

        public FileFullPath FileFullPath
        {
            get { return fileFullPath; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PackagedFileInfo that = (PackagedFileInfo)obj;

            return string.Equals(localPath, that.localPath) && string.Equals(fileFullPath, that.fileFullPath);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
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

        private readonly LocalPath localPath;
        private readonly FileFullPath fileFullPath;
    }
}