using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Flubu.Tasks.Text
{
    /// <summary>
    /// Retrieves a value from an XML file.
    /// </summary>
    /// <remarks>
    /// If provided XPath does not find any matches, null is stored in specified configuration setting.
    /// If provided XPath matches exactly one node, it's value is stored as <see cref="string"/> in specified configuration setting.
    /// If provided XPath matches multiple nodes, their values are stored as <see cref="string"/>[] in specified configuration setting.
    /// </remarks>
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
                    CultureInfo.InvariantCulture, 
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
            get { return true; }
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
        /// <remarks>
        /// If provided <paramref name="xpath"/> does not find any matches, null is stored in configuration setting <paramref name="configurationSettingName"/>.
        /// If provided <paramref name="xpath"/> matches exactly one node, it's value is stored as <see cref="string"/> in configuration setting <paramref name="configurationSettingName"/>.
        /// If provided <paramref name="xpath"/> matches multiple nodes, their values are stored as <see cref="string"/>[] in configuration setting <paramref name="configurationSettingName"/>.
        /// </remarks>
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

            XmlNodeList nodes = xmlDoc.SelectNodes(xpath);
            if (nodes == null || nodes.Count == 0)
            {
                context.Properties.Set<object>(configurationSettingName, null);
                return;
            }

            if (nodes.Count == 1)
            {
                context.Properties.Set(configurationSettingName, nodes[0].InnerText);
            }
            else
            {
                context.Properties.Set(
                    configurationSettingName, 
                    (from XmlNode node in nodes select node.InnerText).ToArray());
            }
        }

        private readonly string xmlFileName;
        private readonly string xpath;
        private readonly string configurationSettingName;
    }
}