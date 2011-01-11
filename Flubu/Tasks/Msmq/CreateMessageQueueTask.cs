using System;
using System.Collections.Generic;
using System.Messaging;

namespace Flubu.Tasks.Msmq
{
    /// <summary>
    /// Creates a message queue with the specified path.
    /// </summary>
    public class CreateMessageQueueTask : TaskBase
    {
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }

        public MessageQueueAccessRights AccessRights
        {
            get { return accessRights; }
            set { accessRights = value; }
        }

        public CreateMessageQueueMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get { return string.Format (System.Globalization.CultureInfo.InvariantCulture, "Create message queue '{0}'", messageQueuePath); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMessageQueueTask"/> class
        /// using a specified message queue path and an indicator of whether the queue
        /// should be transactional or not.
        /// </summary>
        /// <param name="messageQueuePath">The message queue path.</param>
        /// <param name="isTransactional">if set to <c>true</c> the queue will be transactional; otherwise <c>false</c>.</param>
        /// <param name="mode">The task mode.</param>
        public CreateMessageQueueTask (
            string messageQueuePath, 
            bool isTransactional,
            CreateMessageQueueMode mode)
        {
            this.messageQueuePath = messageQueuePath;
            this.isTransactional = isTransactional;
            this.mode = mode;
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            // first check if the queue already exists
            if (MessageQueue.Exists (messageQueuePath))
            {
                if (mode == CreateMessageQueueMode.DoNothingIfExists)
                {
                    context.WriteInfo("Message queue '{0}' already exists, doing nothing.", messageQueuePath);
                    return;
                }

                if (mode == CreateMessageQueueMode.FailIfAlreadyExists)
                    throw new TaskExecutionException (
                        String.Format (
                            System.Globalization.CultureInfo.InvariantCulture,
                            "Message queue '{0}' already exists.", 
                            messageQueuePath));

                // otherwise delete the queue
                context.WriteInfo("Message queue '{0}' already exists, it will be deleted.", messageQueuePath);
                MessageQueue.Delete (messageQueuePath);
            }

            using (MessageQueue queue = MessageQueue.Create (messageQueuePath, isTransactional))
            {
                if (userName != null)
                    queue.SetPermissions (userName, accessRights);
            }
        }

        private string messageQueuePath;
        private bool isTransactional;
        private CreateMessageQueueMode mode;
        private string userName;
        private MessageQueueAccessRights accessRights;
    }
}
