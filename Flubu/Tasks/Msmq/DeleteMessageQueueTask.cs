using System;
using System.Globalization;
using System.Messaging;

namespace Flubu.Tasks.Msmq
{
    public class DeleteMessageQueueTask : TaskBase
    {
        public DeleteMessageQueueTask(string messageQueuePath, bool failIfNotExist)
        {
            MessageQueuePath = messageQueuePath;
            FailIfNotExist = failIfNotExist;
        }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Purge message queue '{0}'", MessageQueuePath); }
        }

        public static void Execute(ITaskContext context, string messageQueuePath, bool failIfNotExist)
        {
            DeleteMessageQueueTask task = new DeleteMessageQueueTask(messageQueuePath, failIfNotExist);
            task.Execute(context);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute(ITaskContext context)
        {
            // first check if the queue already exists
            if (false == MessageQueue.Exists(MessageQueuePath))
            {
                if (FailIfNotExist)
                {
                    throw new TaskExecutionException(
                        String.Format(
                            CultureInfo.InvariantCulture, 
                            "Message queue does not '{0}' exist.", 
                            MessageQueuePath));
                }

                context.WriteInfo(
                    "Message queue does not '{0}' exist, doing nothing.", 
                    MessageQueuePath);
                return;
            }

            MessageQueue.Delete(MessageQueuePath);
        }

        private string MessageQueuePath { get; set; }

        private bool FailIfNotExist { get; set; }
    }
}