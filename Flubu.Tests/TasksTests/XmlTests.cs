using Flubu.Tasks.Text;
using NUnit.Framework;

namespace Flubu.Tests.TasksTests
{
    /// <summary>
    /// Test XML related tasks.
    /// </summary>
    [TestFixture]
    public class XmlTests
    {
        [Test]
        public void FindOne()
        {
            ITaskContext c = new TaskContext (new SimpleTaskContextProperties (), new string[0]);
            ITask peek = new PeekXmlTask(SampleXmlFile, "//id", ConfigurationSetting);
            peek.Execute(c);
            Assert.AreEqual("2011-02-18_12-16-21", c.Properties[ConfigurationSetting]);
        }

        [Test]
        public void FindMultiple()
        {
            ITaskContext c = new TaskContext (new SimpleTaskContextProperties (), new string[0]);
            ITask peek = new PeekXmlTask(SampleXmlFile, "//revision", ConfigurationSetting);
            peek.Execute(c);
            Assert.AreEqual(
                new[] {"2200", "https://svn.server/repos/example1/trunk/src2200", "2200",},
                c.Properties.Get<string[]>(ConfigurationSetting));
        }

        [Test]
        public void FindNone()
        {
            ITaskContext c = new TaskContext (new SimpleTaskContextProperties (), new string[0]);
            ITask peek = new PeekXmlTask(SampleXmlFile, "/trtwe/rwetr", ConfigurationSetting);
            peek.Execute(c);
            Assert.IsNull(c.Properties[ConfigurationSetting]);
        }

        private const string SampleXmlFile = "hudsonBuildSample.xml";
        private const string ConfigurationSetting = "result-34";
    }
}