using System;
using System.Collections.Generic;

namespace Flubu.Services
{
    public class FlubuEnvironmentService : IFlubuEnvironmentService
    {
        public IDictionary<Version, string> ListAvailableMSBuildToolsVersions()
        {
            return FlubuEnvironment.ListAvailableMSBuildToolsVersions();
        }
    }
}