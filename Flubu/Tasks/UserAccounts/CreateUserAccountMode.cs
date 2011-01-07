namespace Flubu.Tasks.UserAccounts
{
    public enum CreateUserAccountMode
    {
        /// <summary>
        /// If the user account already exists, the task should fail.
        /// </summary>
        FailIfAlreadyExists,

        /// <summary>
        /// If the user account already exists, it should be updated.
        /// </summary>
        UpdateIfExists,

        /// <summary>
        /// If the user account already exists, the task should do nothing.
        /// </summary>
        DoNothingIfExists,
    }
}