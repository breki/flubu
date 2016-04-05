using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLine;
using CSScriptLibrary;

namespace Flubu.Console
{
    public class Program
    {
        public static int Main(string[] args)
        {
            List<string> arguments = new List<string>(args);

            string fileName = GetFileName(arguments);
            CSScript.AssemblyResolvingEnabled = true;

            Assembly assembly = CSScript.Load(fileName);

            Type myType = typeof (IBuildScript);
            List<Type> classes = assembly.GetTypes().Where(i => myType.IsAssignableFrom(i)).ToList();
            if (classes.Count <= 0)
            {
                System.Console.WriteLine("Could not find any IBuildScript implementation!");
                return -1;
            }
            try
            {
                object scriptInstance = assembly.CreateInstance(classes[0].FullName);
                IBuildScript script = scriptInstance.AlignToInterface<IBuildScript>();

                Parser p = new Parser();
                p.ParseArguments(args, new object());

                return script.Execute(arguments);
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
                if(File.Exists("buildscript.cs"))
                    return "buildscript.cs";

                if (File.Exists("deployscript.cs"))
                    return "deployscript.cs";

                return Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.cs").FirstOrDefault();
            }

            args.RemoveAll(i => i.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

            return fileArg;
        }
    }
}