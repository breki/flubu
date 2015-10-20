using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.AnalysisTasks
{
    public class RunFxcopTask : TaskBase
    {
        public RunFxcopTask(
            string fxcopCmdExeFileName,
            string fxcopGuiExeFileName,
            string fxcopProjectFileName,
            string fxcopReportFileName)
        {
            this.fxcopCmdExeFileName = fxcopCmdExeFileName;
            this.fxcopGuiExeFileName = fxcopGuiExeFileName;
            this.fxcopProjectFileName = fxcopProjectFileName;
            this.fxcopReportFileName = fxcopReportFileName;
        }

        public override string Description
        {
            get { return "Run FxCop"; }
        }

        protected override void DoExecute(ITaskContext context)
        {
            FileFullPath path = new FileFullPath(fxcopReportFileName);
            path.Directory.EnsureExists();

            RunProgramTask task = new RunProgramTask(fxcopCmdExeFileName, true);
            task
                .AddArgument(@"/project:{0}", fxcopProjectFileName)
                .AddArgument(@"/out:{0}", fxcopReportFileName)
                .AddArgument(@"/dictionary:CustomDictionary.xml")
                .AddArgument(@"/ignoregeneratedcode");
            task.Execute(context);

            // check if the report file was generated
            bool isReportFileGenerated = File.Exists(fxcopReportFileName);

            // FxCop returns different exit codes for different things
            // see http://msdn.microsoft.com/en-us/library/bb164705(VS.80).aspx for the list of exit codes
            // exit code 4 means "Project load error" but it occurs when the old FxCop violations exist
            // which are then removed from the code
            if ((task.LastExitCode != 0 && task.LastExitCode != 4) || isReportFileGenerated)
            {
                if (context.IsInteractive)
                {
                    // run FxCop GUI
                    task = new RunProgramTask(fxcopGuiExeFileName);
                    task
                        .AddArgument(fxcopProjectFileName);
                    task.Execute(context);
                }

                context.Fail("FxCop found violations in the code.");
            }
        }

        private readonly string fxcopCmdExeFileName;
        private readonly string fxcopGuiExeFileName;
        private readonly string fxcopProjectFileName;
        private readonly string fxcopReportFileName;
    }
}