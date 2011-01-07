using System.Collections.Generic;

namespace Flubu.Builds
{
    public abstract class BuildProduct<TRunner>
        where TRunner : FlubuRunner<TRunner>
    {
        public string ProductPartId
        {
            get { return productPartId; }
        }

        public abstract void CopyProductFiles(
            TRunner buildRunner, 
            string packageDirectory);

        public IList<string> ListCopiedFiles()
        {
            return copiedFiles;
        }

        protected BuildProduct (string productPartId)
        {
            this.productPartId = productPartId;
        }

        protected void AddFileToCopiedList (string fileName)
        {
            copiedFiles.Add (fileName);
        }

        protected void AddFilesToCopiedList (IEnumerable<string> fileNames)
        {
            copiedFiles.AddRange(fileNames);
        }

        protected void ClearCopiedFilesList()
        {
            copiedFiles.Clear();
        }

        private readonly string productPartId;
        private readonly List<string> copiedFiles = new List<string>();
    }
}