namespace Flubu.Packaging
{
    public static class LoggingHelper
    {
        public static bool LogIfFilteredOut (string fileName, IFileFilter filter, ITaskContext taskContext)
        {
            if (filter != null && false == filter.IsPassedThrough(fileName))
            {
                taskContext.WriteDebug("File '{0}' has been filtered out.", fileName);
                return false;
            }

            return true;
        }
    }
}