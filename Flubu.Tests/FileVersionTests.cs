using Flubu.Tasks.FileSystem;
using NUnit.Framework;

namespace Flubu.Tests
{
    /// <summary>
    /// Test file version retrieval.
    /// </summary>
    [TestFixture]
    public class FileVersionTests
    {
        /// <summary>
        /// Retrieve .NET assembly version.
        /// </summary>
        [Test]
        public void AssemblyVersion()
        {
            const string Setting = "my-assembly-version";
            ITask t = new GetAssemblyVersionTask("Flubu.Tests.dll", Setting);
            ITaskContext c = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            t.Execute(c);
            Assert.AreEqual("1.4.6754.34", c.Properties[Setting]);
        }

        /// <summary>
        /// Retrieve file version.
        /// </summary>
        [Test]
        public void FileVersion()
        {
            const string Setting = "my-file-version";
            ITask t = new GetFileVersionTask("Flubu.Tests.dll", Setting);
            ITaskContext c = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            t.Execute(c);
            Assert.AreEqual("64.45.85.24", c.Properties[Setting]);
        }

        /// <summary>
        /// File without file version information.
        /// </summary>
        [Test]
        public void NoFileVersion()
        {
            const string Setting = "my-file-version";
            ITask t = new GetFileVersionTask("Flubu.Tests.pdb", Setting);
            ITaskContext c = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            t.Execute(c);
            Assert.IsNull(c.Properties[Setting]);
        }
    }
}