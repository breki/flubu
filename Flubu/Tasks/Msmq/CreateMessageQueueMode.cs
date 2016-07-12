namespace Flubu.Tasks.Msmq
{
    public enum CreateMessageQueueMode
    {
        /// <summary>
        /// If the message queue already exists, the task should fail.
        /// </summary>
        FailIfAlreadyExists, 

        /// <summary>
        /// If the message queue already exists, the message queue should be recreated.
        /// </summary>
        RecreateIfExists, 

        /// <summary>
        /// If the message queue already exists, the task should do nothing.
        /// </summary>
        DoNothingIfExists, 
    }
}