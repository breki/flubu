namespace Flubu.Tasks.UserInterface
{
    public class NotifyUserTask : TaskBase
    {
        public override string Description
        {
            get { return "Notify user"; }
        }

        public NotifyUserTask (string message)
        {
            this.message = message;
        }

        public static void Execute (ITaskContext environment, string message)
        {
            NotifyUserTask task = new NotifyUserTask (message);
            task.Execute (environment);
        }

        protected override void DoExecute (ITaskContext context)
        {
            context.WriteMessage(TaskMessageLevel.Info, message);
        }

        private readonly string message;
    }
}
