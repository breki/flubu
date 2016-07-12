namespace Flubu.Tasks.Misc
{
    internal class Log4NetConfigFile
    {
        public string FileName
        {
            get { return fileName; }
        }

        public string ConfigXpath
        {
            get { return configXpath; }
        }

        public Log4NetConfigFile (string fileName, string configXPath)
        {
            this.fileName = fileName;
            this.configXpath = configXPath;
        }

        private readonly string fileName;
        private readonly string configXpath;
    }
}