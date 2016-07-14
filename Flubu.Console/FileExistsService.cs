using System.IO;

namespace Flubu.Console
{
    public class FileExistsService : IFileExistsService
    {
        public bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }
    }
}