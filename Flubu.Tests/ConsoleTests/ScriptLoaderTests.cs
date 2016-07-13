using System.IO;
using Flubu.Builds;
using Flubu.Console;
using NUnit.Framework;

namespace Flubu.Tests.ConsoleTests
{
    public class ScriptLoaderTests
    {
        [Test]
        public void Test()
        {
            ScriptLoader loader = new ScriptLoader();
            IBuildScript script = loader.FindAndCreateBuildScriptInstance(
                Path.Combine(TestContext.CurrentContext.TestDirectory, @"Samples\BuildScript.cs"));
            Assert.IsNotNull(script);
        }
    }
}