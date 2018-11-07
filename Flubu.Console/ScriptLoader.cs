using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CSScriptLibrary;
using Flubu.Builds;

namespace Flubu.Console
{
    public class ScriptLoader : IScriptLoader
    {
        public IBuildScript FindAndCreateBuildScriptInstance(string fileName)
        {
            CSScript.AssemblyResolvingEnabled = true;
            Assembly assembly = CSScript.LoadFile(fileName);

            Type myType = typeof(IBuildScript);
            List<Type> classes =
                assembly.GetTypes().Where(i => myType.IsAssignableFrom(i)).ToList();
            if (classes.Count <= 0)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Used build script file '{0}' but it does not contain any IBuildScript implementation.",
                    fileName);

                throw new BuildScriptLocatorException(message);
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            object scriptInstance = assembly.CreateInstance(classes[0].FullName);
            return scriptInstance.AlignToInterface<IBuildScript>();
        }
    }
}