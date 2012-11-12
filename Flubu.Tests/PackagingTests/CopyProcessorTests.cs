using Flubu.Packaging;
using NUnit.Framework;
using Rhino.Mocks;

namespace Flubu.Tests.PackagingTests
{
    public class CopyProcessorTests
    {
        [Test, Ignore("todo")]
        public void Test()
        {
            ITaskContext taskContext = MockRepository.GenerateMock<ITaskContext>();
            ICopier copier = MockRepository.GenerateMock<ICopier>();
            FullPath destDir = new FullPath("dest/dir");
            CopyProcessor processor = new CopyProcessor(taskContext, copier, destDir);
            IPackageDef packageDef = MockRepository.GenerateMock<IPackageDef>();

            processor.Process(packageDef);
        }
    }
}