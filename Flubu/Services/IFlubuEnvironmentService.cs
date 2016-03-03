using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace Flubu.Services
{
    /// <summary>
    /// An abstraction layer for various <see cref="FlubuEnvironment"/> utility methods.
    /// </summary>
    [ContractClass(typeof(IFlubuEnvironmentServiceContract))]
    public interface IFlubuEnvironmentService
    {
        /// <summary>
        /// Returns a sorted dictionary of all MSBuild tools versions that are available on the system.
        /// </summary>
        /// <remarks>The method scans through the registry (<c>HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions</c> path)
        /// to find the available tools versions.</remarks>
        /// <returns>A sorted dictionary whose keys are tools versions (2.0, 3.5, 4.0, 12.0 etc.) and values are paths to the
        /// tools directories (and NOT the <c>MSBuild.exe</c> itself!). The entries are sorted ascendingly by version numbers.</returns>
        [NotNull]
        IDictionary<Version, string> ListAvailableMSBuildToolsVersions();
    }

    [ContractClassFor(typeof(IFlubuEnvironmentService))]
    internal abstract class IFlubuEnvironmentServiceContract : IFlubuEnvironmentService
    {
        IDictionary<Version, string> IFlubuEnvironmentService.ListAvailableMSBuildToolsVersions()
        {
            Contract.Ensures(Contract.Result<IDictionary<Version, string>>() != null);
            throw new NotImplementedException();
        }
    }
}