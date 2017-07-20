using System.Collections.Generic;
using System.IO;
using Flubu.Packaging;

namespace Flubu.Tests.PackagingTests
{
    public class MockDirectoryFilesLister : IDirectoryFilesLister
    {
        public MockDirectoryFilesLister List(string fileName)
        {
            filesToList.Add(fileName);
            return this;
        }

        public IEnumerable<string> ListFiles(string directoryName, bool recursive)
        {
            foreach (string file in filesToList)
                yield return Path.Combine(directoryName, file);
        }

        private readonly List<string> filesToList = new List<string>();
    }
}