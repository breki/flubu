using System;
using System.Collections.Generic;
using CommandLine;
using Flubu.Builds;

namespace Flubu.Console
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                IFileExistsService fileExistsService = new FileExistsService();
                IConsoleLogger consoleLogger = new ConsoleLogger();
                IScriptLoader scriptLoader = new ScriptLoader();

                List<string> arguments = new List<string>(args);

                BuildScriptLocator buildScriptLocator =
                    new BuildScriptLocator(fileExistsService, consoleLogger, scriptLoader);

                IBuildScript script =
                    buildScriptLocator.FindBuildScript(arguments);

                Parser p = new Parser();
                p.ParseArguments(args);

                return script.Run(arguments);
            }
            catch (BuildScriptLocatorException ex)
            {
                System.Console.WriteLine("Error executing script {0}.", ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error executing script {0}.", ex.Message);
                return 1;
            }
        }
    }
}