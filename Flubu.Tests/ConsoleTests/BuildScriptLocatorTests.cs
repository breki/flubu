using System.Linq;
using Flubu.Builds;
using Flubu.Console;
using Moq;
using NUnit.Framework;

namespace Flubu.Tests.ConsoleTests
{
    public class BuildScriptLocatorTests
    {
        [Test]
        public void NoArgumentsProvidedAndNoDefaultScripts()
        {
            BuildScriptLocatorException ex = Assert.Throws<BuildScriptLocatorException>(
                () =>
                    {
                        locator.FindBuildScript(new string[] { });
                    });

            StringAssert.Contains(
                "The build script file was not specified. Please specify it as the first argument or use some of the default paths for script file: buildscript.cs", 
                ex.Message);
        }

        [Test]
        public void NoArgumentsProvidedButThereIsDefaultScript()
        {
            const string BuildScriptFileName = "buildscript.cs";
            fileExistsService.Setup(x => x.FileExists(BuildScriptFileName)).Returns(true);
            scriptLoader.Setup(x => x.FindAndCreateBuildScriptInstance(BuildScriptFileName)).Returns(
                new Mock<IBuildScript>().Object);

            locator.FindBuildScript(new string[] { });

            scriptLoader.Verify();
        }

        [Test]
        public void BuildScriptFileNameProvidedButTheFileDoesNotExist()
        {
            const string BuildScriptFileName = "somescript.cs";
            fileExistsService.Setup(x => x.FileExists(BuildScriptFileName)).Returns(false);

            BuildScriptLocatorException ex = Assert.Throws<BuildScriptLocatorException>(
                () =>
                    {
                        locator.FindBuildScript(new[] { BuildScriptFileName });
                    });

            StringAssert.Contains(
                "The build script file specified ('somescript.cs') does not exist.",
                ex.Message);
        }

        [Test]
        public void BuildScriptFileNameProvidedAndTheFileExists()
        {
            const string BuildScriptFileName = "somescript.cs";
            fileExistsService.Setup(x => x.FileExists(BuildScriptFileName))
                .Returns(true);
            scriptLoader.Setup(x => x.FindAndCreateBuildScriptInstance(BuildScriptFileName)).Returns(
                new Mock<IBuildScript>().Object);

            locator.FindBuildScript(new[] { BuildScriptFileName }.ToList());

            scriptLoader.Verify();
        }

        [SetUp]
        public void Setup()
        {
            fileExistsService = new Mock<IFileExistsService>();
            consoleLogger = new Mock<IConsoleLogger>();
            scriptLoader = new Mock<IScriptLoader>();
            locator = new BuildScriptLocator(
                fileExistsService.Object, consoleLogger.Object, scriptLoader.Object);
        }

        private BuildScriptLocator locator;
        private Mock<IFileExistsService> fileExistsService;
        private Mock<IConsoleLogger> consoleLogger;
        private Mock<IScriptLoader> scriptLoader;
    }
}