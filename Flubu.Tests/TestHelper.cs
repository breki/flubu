﻿using System.IO;

namespace Flubu.Tests
{
    public static class TestHelper
    {
        public static string GetRepositoryPath (string path)
        {
            const string RootPath =
#if NCRUNCH
                @"D:\hg\Flubu\"
#else
                @"..\..\..\"
#endif
                ;

            return Path.Combine (RootPath, path);
        }
    }
}