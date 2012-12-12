using Flubu.Tasks.FileSystem;
using NUnit.Framework;

namespace Flubu.Tests
{
    /// <summary>
    /// Test file version retrival.
    /// </summary>
    [TestFixture]
    public class FileVersionTests
    {
        /// <summary>
        /// Retreive .NET asembly version.
        /// </summary>
        [Test]
        public void AssemblyVersion()
        {
            const string setting = "my-assembly-version";
            ITask t = new GetAssemblyVersionTask("Flubu.Tests.dll", setting);
            ITaskContext c = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            t.Execute(c);
            Assert.AreEqual("1.4.6754.34", c.Properties[setting]);
        }

        /// <summary>
        /// Retreive file version.
        /// </summary>
        [Test]
        public void FileVersion()
        {
            const string setting = "my-file-version";
            ITask t = new GetFileVersionTask("Flubu.Tests.dll", setting);
            ITaskContext c = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            t.Execute(c);
            Assert.AreEqual("64.45.85.24", c.Properties[setting]);
        }

        /// <summary>
        /// File without file version information.
        /// </summary>
        [Test]
        public void NoFileVersion()
        {
            const string setting = "my-file-version";
            ITask t = new GetFileVersionTask("Flubu.Tests.pdb", setting);
            ITaskContext c = new TaskContext(new SimpleTaskContextProperties(), new string[0]);
            t.Execute(c);
            Assert.IsNull(c.Properties[setting]);
        }
    }
}