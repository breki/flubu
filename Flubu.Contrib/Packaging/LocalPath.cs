using System;
using System.IO;

namespace Flubu.Packaging
{
    public class LocalPath
    {
        public LocalPath(string path)
        {
            if (Path.IsPathRooted(path))
                throw new ArgumentException("Path must be local", "path");

            localPath = path;
        }

        public LocalPath CombineWith(LocalPath path)
        {
            return new LocalPath(Path.Combine(localPath, path.ToString()));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            LocalPath that = (LocalPath)obj;
            return string.Equals(localPath, that.localPath);
        }

        public override int GetHashCode()
        {
            return localPath.GetHashCode();
        }

        public override string ToString()
        {
            return localPath;
        }

        private readonly string localPath;
    }
}