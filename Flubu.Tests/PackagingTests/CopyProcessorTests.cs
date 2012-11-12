using System;
using System.Collections.Generic;
using Flubu.Packaging;
using NUnit.Framework;
using Rhino.Mocks;

namespace Flubu.Tests.PackagingTests
{
    public class CopyProcessorTests
    {
        [Test]
        public void CopyFiles()
        {
            lister
                .List(@"file_at_root1.txt")
                .List(@"subdir\file_at_root2.txt");

            LocalPath destDir = new LocalPath(@"dest\dir");

            packageDef.AddFolderSource("dir", new FullPath(@"source\dir"), true);

            CopyProcessor processor = new CopyProcessor(taskContext, copier, new FullPath(@"d:\brisi"));
            processor.AddTransformation("dir", destDir);

            processor.Process(packageDef);

            Assert.IsTrue(copier.DestinationFileNames.Contains(@"d:\brisi\dest\dir\file_at_root1.txt"));
            Assert.IsTrue(copier.DestinationFileNames.Contains(@"d:\brisi\dest\dir\subdir\file_at_root2.txt"));
        }

        [Test]
        public void FlattenDirs()
        {
            lister
                .List(@"file_at_root1.txt")
                .List(@"subdir\file_at_root2.txt");
            
            LocalPath destDir = new LocalPath(@"dest\dir");

            packageDef.AddFolderSource("dir", new FullPath(@"source\dir"), true);

            CopyProcessor processor = new CopyProcessor(taskContext, copier, new FullPath(@"d:\brisi"));
            processor.AddTransformationWithDirFlattening("dir", destDir);

            processor.Process(packageDef);

            Assert.IsTrue(copier.DestinationFileNames.Contains(@"d:\brisi\dest\dir\file_at_root1.txt"));
            Assert.IsTrue(copier.DestinationFileNames.Contains(@"d:\brisi\dest\dir\file_at_root2.txt"));
        }

        [SetUp]
        public void Setup()
        {
            taskContext = MockRepository.GenerateMock<ITaskContext>();
            copier = new MockCopier();
            lister = new MockDirectoryFilesLister();
            packageDef = new StandardPackageDef("package", taskContext, lister);
        }

        private MockDirectoryFilesLister lister;
        private ITaskContext taskContext;
        private MockCopier copier;
        private StandardPackageDef packageDef;
    }
}