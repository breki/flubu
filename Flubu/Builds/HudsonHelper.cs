using System;
using System.Globalization;

namespace Flubu.Builds
{
    public static class HudsonHelper
    {
        public static int BuildNumber
        {
            get
            {
                return ParseEnvironmentVariable("BUILD_NUMBER");
            }
        }

        public static bool IsRunningUnderHudson
        {
            get
            {
                string hudsonEnv = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                return hudsonEnv != null;
            }
        }

        /// <summary>
        /// Gets SVN revision number from environment variable.
        /// Normally it is named SVN_REVISION. But under JENKINS when using more than one repository they are named with numbered suffix _1, _2, ...
        /// for each repository respectively (first is named SVN_REVISION_1, ...)
        /// </summary>
        /// <param name="environmentVariableName">Variable name to use.</param>
        /// <returns>SVN Revision number</returns>
        public static int GetSvnRevision(string environmentVariableName)
        {
            if (string.IsNullOrEmpty(environmentVariableName))
                environmentVariableName = "SVN_REVISION";

            return ParseEnvironmentVariable(environmentVariableName);
        }

        public static int ParseEnvironmentVariable (string variableName)
        {
            string valueString = Environment.GetEnvironmentVariable(variableName);
            if (valueString == null)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Environment variable {0} is missing.",
                    variableName);
                throw new InvalidOperationException(message);
            }

            int result;
            if (!int.TryParse(valueString, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Environment variable {0} has an invalid value ('{1}').",
                    variableName,
                    valueString);
                throw new InvalidOperationException(message);
            }

            return result;
        }
    }
}