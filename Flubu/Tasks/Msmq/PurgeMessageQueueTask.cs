using System;
using System.Messaging;

namespace Flubu.Tasks.Msmq
{
    public class PurgeMessageQueueTask : TaskBase
    {
        public PurgeMessageQueueTask(string messageQueuePath)
        {
            this.messageQueuePath = messageQueuePath;
        }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get { return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Purge message queue '{0}'", messageQueuePath); }
        }

        public static void Execute(ITaskContext environment, string messageQueuePath)
        {
            PurgeMessageQueueTask task = new PurgeMessageQueueTask(messageQueuePath);
            task.Execute(environment);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute(ITaskContext context)
        {
            // first check if the queue already exists
            if (false == MessageQueue.Exists(messageQueuePath))
                throw new RunnerFailedException(
                    String.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Message queue does not '{0}' exist.", 
                        messageQueuePath));

            using (MessageQueue queue = new MessageQueue(messageQueuePath))
            {
                queue.Purge();
            }
        }

        private string messageQueuePath;
    }
}