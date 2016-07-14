using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Flubu.Builds;

namespace Flubu.Console
{
    public class BuildScriptLocator
    {
        public BuildScriptLocator(
            IFileExistsService fileExistsService, 
            IConsoleLogger consoleLogger,
            IScriptLoader scriptLoader)
        {
            this.fileExistsService = fileExistsService;
            this.consoleLogger = consoleLogger;
            this.scriptLoader = scriptLoader;
        }

        public IBuildScript FindBuildScript(IList<string> args)
        {
            string fileName = GetFileName(args);

            if (fileName == null)
                ReportUnspecifiedBuildScript();

            return FindAndCreateBuildScriptInstance(fileName);
        }

        private string GetFileName(IList<string> args)
        {
            if (args.Count > 0)
            {
                if (args[0].EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    return TakeExplicitBuildScriptName(args);
            }

            consoleLogger.Log("Build script file name was not explicitly specified, searching the default locations:");

            foreach (string defaultScriptLocation in defaultScriptLocations)
            {
                consoleLogger.Log("Looking for a build script file '{0}'.", defaultScriptLocation);

                if (fileExistsService.FileExists(defaultScriptLocation))
                {
                    consoleLogger.Log("Found it, using the build script file '{0}'.", defaultScriptLocation);
                    return defaultScriptLocation;
                }
            }

            return null;
        }

        private static void ReportUnspecifiedBuildScript()
        {
            StringBuilder errorMsg = new StringBuilder();
            errorMsg.Append(
                "The build script file was not specified. Please specify it as the first argument or use some of the default paths for script file: ");
            foreach (var defaultScriptLocation in defaultScriptLocations)
                errorMsg.AppendLine(defaultScriptLocation);

            throw new BuildScriptLocatorException(errorMsg.ToString());
        }

        private IBuildScript FindAndCreateBuildScriptInstance(string fileName)
        {
            return scriptLoader.FindAndCreateBuildScriptInstance(fileName);
        }

        private string TakeExplicitBuildScriptName(IList<string> args)
        {
            string buildScriptName = args[0];

            if (!fileExistsService.FileExists(buildScriptName))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "The build script file specified ('{0}') does not exist.",
                    buildScriptName);
                throw new BuildScriptLocatorException(message);
            }

            args.RemoveAt(0);
            
            return buildScriptName;
        }

        private static readonly string[] defaultScriptLocations =
        {
            "buildscript.cs",
            "deployscript.cs",
            "buildscript\\buildscript.cs",
            "buildscripts\\buildscript.cs"
        };

        private readonly IFileExistsService fileExistsService;
        private readonly IConsoleLogger consoleLogger;
        private readonly IScriptLoader scriptLoader;
    }
}