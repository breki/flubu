using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Flubu.Tasks.Text
{
    public class ConfigureAppSettingsTask : TaskBase
    {
        public ConfigureAppSettingsTask(string configFileName)
        {
            this.configFileName = configFileName;
        }

        public override string Description
        {
            get
            {
                return String.Format (
                    CultureInfo.InvariantCulture,
                    "Configure application setting in the file '{0}'",
                    configFileName);
            }
        }

        public ConfigureAppSettingsTask SetKey(string key, string value)
        {
            setKeyTasks.Add(new SetKeyTask(key, value));
            return this;
        }

        public ConfigureAppSettingsTask RemoveKey(string key)
        {
            removeKeyTasks.Add(new RemoveKeyTask(key));
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            XmlDocument xmldoc = new XmlDocument ();
            xmldoc.PreserveWhitespace = true;
            xmldoc.Load (configFileName);

            PerformRemoveTasks (xmldoc, context);
            PerformSetTasks (xmldoc, context);

            xmldoc.Save (configFileName);
        }

        private void PerformRemoveTasks(XmlDocument xmldoc, ITaskContext context)
        {
            foreach (RemoveKeyTask task in removeKeyTasks)
            {
                context.WriteInfo ("App setting key '{0}' will be removed.", task.Key);
                
                string xpath = string.Format (CultureInfo.InvariantCulture, "/configuration/appSettings/add[@key='{0}']", task.Key);
                foreach (XmlNode node in xmldoc.SelectNodes (xpath))
                    node.ParentNode.RemoveChild(node);
            }
        }

        private void PerformSetTasks(XmlDocument xmldoc, ITaskContext context)
        {
            foreach (SetKeyTask task in setKeyTasks)
            {
                string xpath = string.Format (CultureInfo.InvariantCulture, "/configuration/appSettings/add[@key='{0}']", task.Key);
                bool wasFound = false;

                foreach (XmlNode node in xmldoc.SelectNodes(xpath))
                {
                    wasFound = true;
                    node.Attributes["value"].Value = task.Value;

                    context.WriteInfo ("App setting key '{0}' will be updated.", task.Key);
                }

                if (wasFound)
                    continue;

                XmlElement addEl = CreateElement(xmldoc, "add");
                AddAttribute(addEl, "key", task.Key);
                AddAttribute(addEl, "value", task.Value);

                XmlNode appSettingsEl = GetOrCreateAppSettingsElement(xmldoc);
                appSettingsEl.AppendChild(addEl);

                context.WriteInfo ("App setting key '{0}' will be added.", task.Key);
            }
        }

        private static XmlNode GetOrCreateAppSettingsElement(XmlDocument xmldoc)
        {
            XmlNode appSettingsEl = xmldoc.SelectSingleNode("/configuration/appSettings");

            if (appSettingsEl == null)
            {
                appSettingsEl = CreateElement(xmldoc, "appSettings");
                xmldoc.SelectSingleNode("/configuration").AppendChild(appSettingsEl);
            }

            return appSettingsEl;
        }

        private static XmlElement CreateElement(XmlDocument xmldoc, string elementName)
        {
            return xmldoc.CreateElement (elementName);
        }

        private static void AddAttribute(XmlNode element, string attributeName, string attributeValue)
        {
            XmlAttribute att = element.OwnerDocument.CreateAttribute(attributeName);
            att.Value = attributeValue;
            element.Attributes.Append(att);
        }

        private readonly string configFileName;
        private List<SetKeyTask> setKeyTasks = new List<SetKeyTask>();
        private List<RemoveKeyTask> removeKeyTasks = new List<RemoveKeyTask>();

        private class SetKeyTask
        {
            public SetKeyTask(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public string Key
            {
                get { return key; }
            }

            public string Value
            {
                get { return value; }
            }

            private string key;
            private string value;
        }

        private class RemoveKeyTask
        {
            public RemoveKeyTask(string key)
            {
                this.key = key;
            }

            public string Key
            {
                get { return key; }
            }

            private readonly string key;
        }
    }
}