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

        // NOTE: MERCURIAL_REVISION is not a number
        //public static int MercurialRevision
        //{
        //    get
        //    {
        //        return ParseEnvironmentVariable("MERCURIAL_REVISION");
        //    }
        //}

        public static int SvnRevision
        {
            get
            {
                return ParseEnvironmentVariable("SVN_REVISION");
            }
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