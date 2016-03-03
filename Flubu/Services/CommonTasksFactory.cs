using Flubu.Tasks.Processes;
using Flubu.Tasks.UserAccounts;

namespace Flubu.Services
{
    public class CommonTasksFactory : ICommonTasksFactory
    {
        public IRunProgramTask CreateRunProgramTask(string programExePath, bool ignoreExitCodes = false)
        {
            return new RunProgramTask(programExePath, ignoreExitCodes);
        }
    }
}