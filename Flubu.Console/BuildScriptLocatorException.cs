using System;

namespace Flubu.Console
{
    [Serializable]
    public class BuildScriptLocatorException : Exception
    {
        public BuildScriptLocatorException(string message)
            : base(message)
        {
        }
    }
}