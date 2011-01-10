using System;
using System.IO;

namespace Flubu
{
    public class FileFullPath : IPathBuilder
    {
        public FileFullPath(string fileName)
        {
            if (Path.IsPathRooted(fileName))
                this.fileName = fileName;
            else
                this.fileName = Path.GetFullPath(fileName);
        }

        public string FileName
        {
            get { return Path.GetFileName(fileName); }
        }

        public FullPath Directory
        {
            get { return ToFullPath().ParentPath; }
        }

        public int Length
        {
            get { return fileName.Length; }
        }

        public override string ToString()
        {
            return fileName;
        }

        public FullPath ToFullPath()
        {
            return new FullPath(fileName);
        }

        private readonly string fileName;
    }
}