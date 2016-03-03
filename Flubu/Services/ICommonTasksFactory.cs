using System.Diagnostics.Contracts;
using Flubu.Tasks.Processes;
using JetBrains.Annotations;

namespace Flubu.Services
{
    /// <summary>
    /// Factory for some Flubu tasks.
    /// </summary>
    /// <remarks>This factory is used as an abstraction layer so the tasks can be more testable.</remarks>
    [ContractClass(typeof(ICommonTasksFactoryContract))]
    public interface ICommonTasksFactory
    {
        [NotNull]
        IRunProgramTask CreateRunProgramTask (string programExePath, bool ignoreExitCodes = false);
    }

    [ContractClassFor(typeof(ICommonTasksFactory))]
    internal abstract class ICommonTasksFactoryContract : ICommonTasksFactory
    {
        IRunProgramTask ICommonTasksFactory.CreateRunProgramTask ([NotNull] string programExePath, bool ignoreExitCodes)
        {
            Contract.Requires(programExePath != null);
            Contract.Ensures(Contract.Result<IRunProgramTask>() != null);
            throw new System.NotImplementedException();
        }
    }
}