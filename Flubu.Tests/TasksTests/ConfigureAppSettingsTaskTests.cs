using System.IO;
using System.Linq;
using Flubu.Tasks.Text;
using NUnit.Framework;

namespace Flubu.Tests.TasksTests
{
    public class ConfigureAppSettingsTaskTests
    {
        [Test]
        public void AddElementToEmptyFile()
        {
            const string FileName = "config.xml";

            PrepareFile(FileName, @"<configuration></configuration>");

            task = new ConfigureAppSettingsTask (FileName);
            task.SetKey("my-setting", "my-value");
            task.Execute(taskContext);

            AssertXmlResult (FileName, @"<configuration><appSettings><add key='my-setting' value='my-value' /></appSettings></configuration>");
        }

        [Test]
        public void ReplaceValue()
        {
            const string FileName = "config.xml";

            PrepareFile (FileName, @"<configuration><appSettings><add key='my-setting' value='my-value' /></appSettings></configuration>");

            task = new ConfigureAppSettingsTask (FileName);
            task.SetKey ("my-setting", "somethingelse");
            task.Execute(taskContext);

            AssertXmlResult (FileName, @"<configuration><appSettings><add key='my-setting' value='somethingelse' /></appSettings></configuration>");
        }

        [Test]
        public void RemoveValue()
        {
            const string FileName = "config.xml";

            PrepareFile (FileName, @"<configuration><appSettings><add key='my-setting' value='my-value' /></appSettings></configuration>");

            task = new ConfigureAppSettingsTask (FileName);
            task.RemoveKey("my-setting");
            task.Execute(taskContext);

            AssertXmlResult (FileName, @"<configuration><appSettings></appSettings></configuration>");
        }

        [Test]
        public void TryToRemoveValueThatIsNotThere()
        {
            const string FileName = "config.xml";

            PrepareFile (FileName, @"<configuration><appSettings><add key='my-setting' value='my-value' /></appSettings></configuration>");

            task = new ConfigureAppSettingsTask (FileName);
            task.RemoveKey("my-setting2");
            task.Execute(taskContext);

            AssertXmlResult (FileName, @"<configuration><appSettings><add key='my-setting' value='my-value' /></appSettings></configuration>");
        }

        [SetUp]
        public void Setup()
        {
            taskContext = new TaskContext(null, Enumerable.Empty<string>());
        }

        private static void PrepareFile(string fileName, string xml)
        {
            File.WriteAllText (fileName, xml);
        }

        private static void AssertXmlResult(string fileName, string expectedXml)
        {
            Assert.AreEqual(expectedXml, File.ReadAllText(fileName).Replace("\"", "'"));
        }

        private ConfigureAppSettingsTask task;
        private ITaskContext taskContext;
    }
}