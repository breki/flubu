using System;

namespace Flubu.Packaging
{
    public interface ICopier
    {
        void Copy(string sourceFileName, string destinationFileName);
    }
}