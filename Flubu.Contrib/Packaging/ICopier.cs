using System;

namespace Flubu.Packaging
{
    public interface ICopier
    {
        void Copy(FileFullPath sourceFileName, FileFullPath destinationFileName);
    }
}