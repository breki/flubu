using System;
using System.Xml;

namespace Flubu.Tasks.Text
{
    /// <summary>
    /// Retrieves a value from an XML file.
    /// </summary>
    public class PeekXmlTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return String.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Read xpath '{0}' from file '{1}' and store it into '{2}' setting.",
                    xpath, 
                    xmlFileName, 
                    configurationSettingName);
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

        public PeekXmlTask(
            string xmlFileName,
            string xpath,
            string configurationSettingName)
        {
            this.xmlFileName = xmlFileName;
            this.xpath = xpath;
            this.configurationSettingName = configurationSettingName;
        }

        /// <summary>
        /// Reads a specified value from an XML file and stores it as a specified configuration setting.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="xmlFileName">The name of the configuration file.</param>
        /// <param name="xpath">The xpath of the value to read.</param>
        /// <param name="configurationSettingName">Name of the configuration setting into which the XML value will be stored.</param>
        public static void Execute(
            ITaskContext environment,
            string xmlFileName,
            string xpath,
            string configurationSettingName)
        {
            if (environment == null)
                throw new ArgumentNullException("environment");

            if (xmlFileName == null)
                throw new ArgumentNullException("xmlFileName");

            if (xpath == null)
                throw new ArgumentNullException("xpath");

            if (configurationSettingName == null)
                throw new ArgumentNullException("configurationSettingName");

            PeekXmlTask task = new PeekXmlTask(xmlFileName, xpath, configurationSettingName);
            task.Execute(environment);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute(ITaskContext context)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFileName);

            XmlNode node = xmlDoc.SelectSingleNode("Configuration");

            if (node != null)
                context.Properties.Set(configurationSettingName, node.InnerText);
        }

        private string xmlFileName;
        private string xpath;
        private string configurationSettingName;
    }
}