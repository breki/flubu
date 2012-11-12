using System.Collections.Generic;
using System.Linq;
using Flubu.Packaging;

namespace Flubu.Tests.PackagingTests
{
    public class MockCopier : ICopier
    {
        public IList<KeyValuePair<string, string>> CopiedFiles
        {
            get { return copiedFiles; }
        }

        public HashSet<string> DestinationFileNames
        {
            get { return new HashSet<string>(copiedFiles.AsQueryable().Select(x => x.Value));}
        }

        public void Copy(FileFullPath sourceFileName, FileFullPath destinationFileName)
        {
            copiedFiles.Add(new KeyValuePair<string, string>(sourceFileName.ToString(), destinationFileName.ToString()));
        }

        public bool HasFile(string sourceFileName, string destinationFileName)
        {
            return
                copiedFiles.Contains(new KeyValuePair<string, string>(sourceFileName, destinationFileName));
        }

        private List<KeyValuePair<string, string>> copiedFiles
            = new List<KeyValuePair<string, string>>();
    }
}