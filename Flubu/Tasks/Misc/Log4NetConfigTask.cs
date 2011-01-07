using System;
using System.Collections.Generic;
using Flubu.Tasks.Text;

namespace Flubu.Tasks.Misc
{
    public class Log4NetConfigTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get 
            {
                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Update log4net configuration files");
            }
        }

        /// <summary>
        /// Gets or sets the root priority. 
        /// </summary>
        /// <value>The root priority. If set to <c>null</c>, the task does not modify the existing values in config files.</value>
        public string RootPriority
        {
            get { return rootPriority; }
            set { rootPriority = value; }
        }

        /// <summary>
        /// Gets or sets the AppendToFile setting for the appenders.
        /// </summary>
        /// <value>The AppendToFile setting for the appenders. If set to <c>null</c>,
        /// the setting will not be modified in configuration files.</value>
        public bool? AppendToFile
        {
            get { return appendToFile; }
            set { appendToFile = value; }
        }

        /// <summary>
        /// Adds the config file to the list of config files to be configured.
        /// </summary>
        /// <param name="configFileName">Name of the config file.</param>
        /// <param name="log4NetConfigXpath">XPath to the log4net configuration block.</param>
        public void AddConfigFile (string configFileName, string log4NetConfigXpath)
        {
            configFiles.Add (new Log4NetConfigFile (configFileName, log4NetConfigXpath));
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            foreach (Log4NetConfigFile file in configFiles)
            {
                UpdateXmlFileTask task = new UpdateXmlFileTask (file.FileName);

                string appenderXpath = String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0}/appender", 
                    file.ConfigXpath);

                if (appendToFile.HasValue)
                {
                    string appendToFileParamXpath = String.Format (
                        System.Globalization.CultureInfo.InvariantCulture,
                        "{0}/param[@name='AppendToFile']/@value", 
                        appenderXpath);

                    task.UpdatePath (appendToFileParamXpath, appendToFile.Value.ToString());
                }

                string rootXpath = String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "{0}/root", 
                    file.ConfigXpath);

                if (rootPriority != null)
                {
                    string priorityXpath = String.Format (
                        System.Globalization.CultureInfo.InvariantCulture,
                        "{0}/priority/@value", 
                        rootXpath);

                    task.UpdatePath (priorityXpath, rootPriority);
                }

                task.Execute (context);
            }
        }

        private readonly IList<Log4NetConfigFile> configFiles = new List<Log4NetConfigFile> ();
        private string rootPriority;
        private bool? appendToFile;
    }
}
