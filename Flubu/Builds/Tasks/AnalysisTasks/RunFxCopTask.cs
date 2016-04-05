using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.AnalysisTasks
{
    public class RunFxCopTask : TaskBase
    {
        public RunFxCopTask(
            string fxCopCmdExeFileName,
            string fxCopGuiExeFileName,
            string fxCopProjectFileName,
            string fxCopReportFileName)
        {
            this.fxCopCmdExeFileName = fxCopCmdExeFileName;
            this.fxCopGuiExeFileName = fxCopGuiExeFileName;
            this.fxCopProjectFileName = fxCopProjectFileName;
            this.fxCopReportFileName = fxCopReportFileName;
        }

        public override string Description
        {
            get { return "Run FxCop"; }
        }

        protected override void DoExecute(ITaskContext context)
        {
            FileFullPath path = new FileFullPath(fxCopReportFileName);
            path.Directory.EnsureExists();

            RunProgramTask task = new RunProgramTask(fxCopCmdExeFileName, true);
            task
                .AddArgument(@"/project:{0}", fxCopProjectFileName)
                .AddArgument(@"/out:{0}", fxCopReportFileName)
                .AddArgument(@"/dictionary:CustomDictionary.xml")
                .AddArgument(@"/ignoregeneratedcode");
            task.Execute(context);

            // check if the report file was generated
            bool isReportFileGenerated = File.Exists(fxCopReportFileName);

            // FxCop returns different exit codes for different things
            // see http://msdn.microsoft.com/en-us/library/bb164705(VS.80).aspx for the list of exit codes
            // exit code 4 means "Project load error" but it occurs when the old FxCop violations exist
            // which are then removed from the code
            if ((task.LastExitCode != 0 && task.LastExitCode != 4) || isReportFileGenerated)
            {
                if (context.IsInteractive)
                {
                    // run FxCop GUI
                    task = new RunProgramTask(fxCopGuiExeFileName);
                    task
                        .AddArgument(fxCopProjectFileName);
                    task.Execute(context);
                }

                context.Fail("FxCop found violations in the code.");
            }
        }

        private readonly string fxCopCmdExeFileName;
        private readonly string fxCopGuiExeFileName;
        private readonly string fxCopProjectFileName;
        private readonly string fxCopReportFileName;
    }
}