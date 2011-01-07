using System;
using System.IO;

namespace Flubu.Packaging
{
    public class FullPath
    {
        public FullPath(string path)
        {
            if (Path.IsPathRooted(path))
                fullPath = path;
            else
                fullPath = Path.GetFullPath(path);
        }

        public FullPath CombineWith (LocalPath localPath)
        {
            return new FullPath(Path.Combine(fullPath, localPath.ToString()));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            FullPath that = (FullPath) obj;
            return string.Equals(fullPath, that.fullPath);
        }

        public override int GetHashCode()
        {
            return fullPath.GetHashCode();
        }

        public override string ToString()
        {
            return fullPath;
        }

        private string fullPath;
    }
}