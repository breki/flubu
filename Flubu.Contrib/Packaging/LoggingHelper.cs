namespace Flubu.Packaging
{
    public static class LoggingHelper
    {
        public static bool LogIfFilteredOut (string fileName, IFileFilter filter, ILogger logger)
        {
            if (filter != null && false == filter.IsPassedThrough(fileName))
            {
                logger.Log("File '{0}' has been filtered out.", fileName);
                return false;
            }

            return true;
        }
    }
}