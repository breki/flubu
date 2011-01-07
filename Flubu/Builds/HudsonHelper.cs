using System;

namespace Flubu.Builds
{
    public static class HudsonHelper
    {
        public static bool IsRunningUnderHudson
        {
            get
            {
                string hudsonEnv = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                return hudsonEnv != null;
            }
        }
    }
}