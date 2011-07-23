using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Flubu.Tasks.Text
{
    /// <summary>
    /// Updates an XML file using the specified update commands.
    /// </summary>
    public class UpdateXmlFileTask : TaskBase
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
                    "Update XML file '{0}'", 
                    fileName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateXmlFileTask"/> class with
        /// the specified XML file to be updated.
        /// </summary>
        /// <param name="fileName">The fileName of the XML file.</param>
        public UpdateXmlFileTask (string fileName)
        {
            this.fileName = fileName;
        }

        /// <summary>
        /// Adds an "update" command to the list of commands to be performed on the XML file.
        /// </summary>
        /// <param name="xpath">XPath for the nodes which should be updated.</param>
        /// <param name="value">New value of the selected nodes.</param>
        public void UpdatePath (string xpath, string value)
        {
            xpathUpdates.Add (xpath, value);
        }

        /// <summary>
        /// Adds an "delete" command to the list of commands to be performed on the XML file.
        /// </summary>
        /// <param name="xpath">XPath for the nodes which should be deleted.</param>
        public void DeletePath (string xpath)
        {
            xpathDeletes.Add (xpath);
        }

        /// <summary>
        /// Adds an "add" command to the list of commands to be performed on the XML file.
        /// </summary>
        /// <param name="rootXpath">XPath for the root node on which an addition should be performed.</param>
        /// <param name="childNodeName">Name of the new child node.</param>
        /// <param name="value">The value for the new child node.</param>
        public void AddPath (string rootXpath, string childNodeName, string value)
        {
            UpdateXmlFileTaskAddition addition = new UpdateXmlFileTaskAddition (rootXpath, childNodeName, value);
            xpathAdditions.Add (addition);
        }

        public void AddPath (string rootXpath, string childNodeName)
        {
            UpdateXmlFileTaskAddition addition = new UpdateXmlFileTaskAddition (rootXpath, childNodeName);
            xpathAdditions.Add (addition);
        }

        public void AddPath (string rootXpath, string childNodeName, IDictionary<string, string> attributes)
        {
            UpdateXmlFileTaskAddition addition = new UpdateXmlFileTaskAddition (rootXpath, childNodeName, attributes);
            xpathAdditions.Add (addition);
        }

        /// <summary>
        /// Method defining the actual work for a task.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            XmlDocument xmldoc = new XmlDocument ();
            xmldoc.PreserveWhitespace = true;
            xmldoc.Load (fileName);

            PerformDeletes(xmldoc, context);
            PerformUpdates(xmldoc, context);
            PerformAdditions(xmldoc, context);

            xmldoc.Save (fileName);
        }

        private void PerformDeletes(XmlDocument xmldoc, ITaskContext context)
        {
            foreach (string xpath in xpathDeletes)
            {
                foreach (XmlNode node in xmldoc.SelectNodes (xpath))
                {
                    string fullNodePath = ConstructXmlNodeFullName (node);

                    context.WriteInfo("Node '{0}' will be removed.", fullNodePath);

                    if (node.NodeType == XmlNodeType.Element)
                        node.ParentNode.RemoveChild (node);
                    else if (node.NodeType == XmlNodeType.Attribute)
                    {
                        XmlAttribute attribute = (XmlAttribute)node;
                        attribute.OwnerElement.RemoveAttributeNode (attribute);
                    }
                    else
                        throw new ArgumentException (
                            String.Format (
                                System.Globalization.CultureInfo.InvariantCulture,
                                "Node '{0}' is of incorrect type '{1}', it should be an element or attribute.",
                                fullNodePath, 
                                node.NodeType));
                }
            }
        }

        private void PerformUpdates(XmlDocument xmldoc, ITaskContext context)
        {
            foreach (string xpath in xpathUpdates.Keys)
                foreach (XmlNode node in xmldoc.SelectNodes (xpath))
                    UpdateNode(xpath, node, context);
        }

        private void UpdateNode(string xpath, XmlNode node, ITaskContext context)
        {
            string fullNodePath = ConstructXmlNodeFullName (node);

            context.WriteInfo(
                "Node '{0}' will have value '{1}'", 
                fullNodePath, 
                xpathUpdates[xpath]);

            if (node.NodeType == XmlNodeType.Attribute)
                node.Value = xpathUpdates[xpath];
            else if (node.NodeType == XmlNodeType.Element)
                node.InnerText = xpathUpdates[xpath];
            else
                throw new ArgumentException (
                    String.Format (
                        System.Globalization.CultureInfo.InvariantCulture,
                        "Node '{0}' is of incorrect type '{1}', it should be an element or attribute.",
                        fullNodePath, 
                        node.NodeType));
        }

        private void PerformAdditions(XmlDocument xmldoc, ITaskContext context)
        {
            foreach (UpdateXmlFileTaskAddition addition in xpathAdditions)
            {
                XmlNode rootNode = xmldoc.SelectSingleNode (addition.RootXPath);

                if (rootNode == null)
                    throw new ArgumentException (
                        String.Format (
                            System.Globalization.CultureInfo.InvariantCulture,
                            "Path '{0}' does not exist.", 
                            addition.RootXPath));

                if (rootNode.NodeType != XmlNodeType.Element)
                    throw new ArgumentException (
                        String.Format (
                            System.Globalization.CultureInfo.InvariantCulture,
                            "Node '{0}' is of incorrect type '{1}', it should be an element.",
                            addition.RootXPath, 
                            rootNode.NodeType));

                XmlNode childNode = null;
                if (addition.ChildNodeName.StartsWith ("@", StringComparison.OrdinalIgnoreCase))
                {
                    childNode = xmldoc.CreateAttribute (addition.ChildNodeName.Substring (1));
                    childNode.Value = addition.Value;
                    rootNode.Attributes.Append ((XmlAttribute)childNode);
                }
                else
                {
                    childNode = xmldoc.CreateElement (addition.ChildNodeName);
                    if (addition.Value != null)
                        childNode.InnerText = addition.Value;

                    if (addition.Attributes != null)
                    {
                        XmlElement element = (XmlElement)childNode;

                        foreach (string attribute in addition.Attributes.Keys)
                            element.SetAttribute (attribute, addition.Attributes[attribute]);
                    }

                    rootNode.AppendChild (childNode);
                }

                string fullNodePath = ConstructXmlNodeFullName (rootNode);

                context.WriteInfo(
                    "Node '{0}' will have a new child '{1}' with value '{2}'", 
                    fullNodePath, 
                    childNode.Name, 
                    addition.Value);
            }
        }

        private static string ConstructXmlNodeFullName(XmlNode node)
        {
            StringBuilder fullNodePath = new StringBuilder ();
            string terminator = null;

            XmlNode node2 = node;
            while (node2 != null)
            {
                fullNodePath.Insert (0, terminator);
                fullNodePath.Insert (0, node2.Name);
                if (node2.NodeType == XmlNodeType.Attribute)
                    fullNodePath.Insert (0, '@');
                terminator = "/";

                if (node2.NodeType == XmlNodeType.Element)
                    node2 = node2.ParentNode;
                else if (node2.NodeType == XmlNodeType.Attribute)
                    node2 = ((XmlAttribute)node2).OwnerElement;
                else
                    node2 = null;
            }

            return fullNodePath.ToString ();
        }

        private string fileName;
        private Dictionary<string, string> xpathUpdates = new Dictionary<string, string> ();
        private List<string> xpathDeletes = new List<string> ();
        private List<UpdateXmlFileTaskAddition> xpathAdditions = new List<UpdateXmlFileTaskAddition> ();

        internal class UpdateXmlFileTaskAddition
        {
            public string RootXPath
            {
                get { return rootXPath; }
            }

            public string ChildNodeName
            {
                get { return childNodeName; }
            }

            public string Value
            {
                get { return value; }
            }

            internal IDictionary<string, string> Attributes
            {
                get { return attributes; }
            }

            internal UpdateXmlFileTaskAddition (string rootXPath, string childNodeName, string value)
            {
                this.rootXPath = rootXPath;
                this.childNodeName = childNodeName;
                this.value = value;
            }

            internal UpdateXmlFileTaskAddition (string rootXPath, string childNodeName)
            {
                this.rootXPath = rootXPath;
                this.childNodeName = childNodeName;
            }

            internal UpdateXmlFileTaskAddition (string rootXPath, string childNodeName, IDictionary<string, string> attributes)
            {
                this.rootXPath = rootXPath;
                this.childNodeName = childNodeName;
                this.attributes = new Dictionary<string, string> (attributes);
            }

            private string rootXPath;
            private string childNodeName;
            private string value;
            private Dictionary<string, string> attributes;
        }
    }
}
