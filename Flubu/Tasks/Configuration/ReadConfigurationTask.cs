using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Flubu.Tasks.Configuration
{
    /// <summary>
    /// Reads a configuration in XML form and stores it into <see cref="ITaskContext"/>
    /// configuration settings;
    /// </summary>
    public class ReadConfigurationTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string TaskDescription
        {
            get
            {
                if (configurationString != null)
                    return String.Format (
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Read configuration string: '{0}'",
                        configurationString);
                if (configurationFileName != null)
                    return String.Format (
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Read configuration file: '{0}'",
                        configurationFileName);

                return "Read configuration";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is safe to execute in dry run mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is safe to execute in dry run mode; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSafeToExecuteInDryRun
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Reads configuration from a string.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="configurationString">The configuration string.</param>
        public static void ReadFromString (ITaskContext environment, string configurationString)
        {
            if (environment == null)
                throw new ArgumentNullException ("environment");

            if (configurationString == null)
                throw new ArgumentNullException ("configurationString");                

            ReadConfigurationTask task = new ReadConfigurationTask ();
            task.configurationString = configurationString;
            task.Execute (environment);
        }

        /// <summary>
        /// Reads configuration from a file.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="configurationFileName">The name of the configuration file.</param>
        public static void ReadFromFile (ITaskContext environment, string configurationFileName)
        {
            if (environment == null)
                throw new ArgumentNullException ("environment");

            if (configurationFileName == null)
                throw new ArgumentNullException ("configurationFileName");                
            
            ReadConfigurationTask task = new ReadConfigurationTask ();
            task.configurationFileName = configurationFileName;
            task.Execute (environment);
        }

        /// <summary>
        /// Reads configuration from a configuration file which has the same name as the running script.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        public static void ReadFromScriptConfigurationFile(ITaskContext environment)
        {
            if (environment == null)
                throw new ArgumentNullException ("environment");                
            
            ReadConfigurationTask task = new ReadConfigurationTask ();
            string configFileName = String.Format (
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}.config", 
                environment.ScriptName);
            task.configurationFileName = Path.Combine (
                Environment.CurrentDirectory, 
                configFileName);
            task.Execute (environment);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            XmlDocument xmlDoc = new XmlDocument ();
            if (configurationString != null)
                xmlDoc.LoadXml (configurationString);
            else if (configurationFileName != null)
            {
                xmlDoc.Load (configurationFileName);
            }
            else
                throw new RunnerFailedException ("Either the configuration string or the configuration fileName has to be set.");

            XmlNode configurationRootNode = xmlDoc.SelectSingleNode ("Configuration");

            XmlNodeList nodes = xmlDoc.SelectNodes ("Configuration//*");
            foreach (XmlNode node in nodes)
            {
                if (node.InnerText == node.InnerXml)
                {
                    StringBuilder settingName = new StringBuilder ();
                    string terminator = null;
                    for (XmlNode parentNode = node; parentNode != null && parentNode != configurationRootNode; parentNode = parentNode.ParentNode)
                    {
                        settingName.Insert (0, terminator);
                        settingName.Insert (0, parentNode.Name);
                        terminator = "/";
                    }

                    context.WriteInfo(
                        "Configuration setting '{0}' has value '{1}'", 
                        settingName, 
                        node.InnerText);
                    context.SetConfigSetting (settingName.ToString (), node.InnerText);
                }
            }
        }

        private string configurationString;
        private string configurationFileName;
    }
}
