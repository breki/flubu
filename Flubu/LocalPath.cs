using System;
using System.IO;

namespace Flubu
{
    public class LocalPath : IPathBuilder
    {
        public LocalPath(string path)
        {
            if (Path.IsPathRooted(path))
                throw new ArgumentException("Path must be local", "path");

            localPath = path;
        }

        public string FileName
        {
            get { return Path.GetFileName(localPath); }
        }

        public LocalPath Flatten
        {
            get { return new LocalPath(FileName); }
        }

        public int Length
        {
            get { return localPath.Length; }
        }

        /// <summary>
        /// Gets the path that is a parent to the current path in this object.
        /// </summary>
        /// <value>The parent path.</value>
        public LocalPath ParentPath
        {
            get
            {
                return new LocalPath(Path.GetDirectoryName(localPath));
            }
        }

        public static implicit operator string (LocalPath path)
        {
            return path.ToString ();
        }

        public LocalPath CombineWith(LocalPath path)
        {
            return new LocalPath(Path.Combine(localPath, path.ToString()));
        }

        public LocalPath CombineWith (string path)
        {
            return CombineWith (new LocalPath (path));
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