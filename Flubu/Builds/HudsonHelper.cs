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
                string revisionNumberString = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                return int.Parse(revisionNumberString, CultureInfo.InvariantCulture);
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

        public static int MercurialRevision
        {
            get
            {
                string revisionNumberString = Environment.GetEnvironmentVariable("MERCURIAL_REVISION");
                return int.Parse(revisionNumberString, CultureInfo.InvariantCulture);
            }
        }

        public static int SvnRevision
        {
            get
            {
                string svnRevisionNumberString = Environment.GetEnvironmentVariable("SVN_REVISION");
                return int.Parse(svnRevisionNumberString, CultureInfo.InvariantCulture);
            }
        }
    }
}