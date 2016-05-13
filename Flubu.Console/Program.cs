using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CSScriptLibrary;
using Flubu.Builds;

namespace Flubu.Console
{
    public class Program
    {
        private static List<string> defaultScriptLocations = new List<string>
        {
            "buildscript.cs",
            "deployscript.cs",
            "buildscript\\buildscript.cs",
            "buildscripts\\buildscript.cs"
        };

        public static int Main(string[] args)
        {
            try
            {
                List<string> arguments = new List<string>(args);

                string fileName = GetFileName(arguments);

                if (fileName == null)
                {
                    System.Console.WriteLine(
                        ".cs script file not specified. Specify it as argument or use some of the default paths for script file:");
                    foreach (var defaultScriptLocation in defaultScriptLocations)
                    {
                        System.Console.WriteLine(defaultScriptLocation);
                    }

                    return -1;
                }

                CSScript.AssemblyResolvingEnabled = true;
                Assembly assembly = CSScript.Load(fileName);

                Type myType = typeof (IBuildScript);
                List<Type> classes = assembly.GetTypes().Where(i => myType.IsAssignableFrom(i)).ToList();
                if (classes.Count <= 0)
                {
                    System.Console.WriteLine("Could not find any IBuildScript implementation!");
                    return -1;
                }
                object scriptInstance = assembly.CreateInstance(classes[0].FullName);
                IBuildScript script = scriptInstance.AlignToInterface<IBuildScript>();

                Parser p = new Parser();
                p.ParseArguments(args, new object());

                return script.Run(arguments);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error executing script {0}.", e.Message);
                return -1;
            }
        }

        private static string GetFileName(List<string> args)
        {
            string fileArg = args.FirstOrDefault(i => i.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(fileArg))
            {
                foreach (var defaultScriptLocation in defaultScriptLocations)
                {
                    if (File.Exists(defaultScriptLocation))
                    {
                        return defaultScriptLocation;
                    }
                }

                return Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.cs").FirstOrDefault();
            }

            args.RemoveAll(i => i.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

            return fileArg;
        }
    }
}