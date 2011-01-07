using System;
using System.Collections.Generic;
using System.Globalization;

namespace Flubu
{
    public class SimpleTaskContextProperties : ITaskContextProperties
    {
        public T Get<T>(string propertyName)
        {
            if (!properties.ContainsKey(propertyName))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Task context property '{0}' is missing.",
                    propertyName);
                throw new KeyNotFoundException(message);
            }

            return (T)properties[propertyName];
        }

        public T Get<T>(string propertyName, T defaultValue)
        {
            if (!properties.ContainsKey(propertyName))
                return defaultValue;
            return Get<T>(propertyName);
        }

        public bool Has(string propertyName)
        {
            return properties.ContainsKey(propertyName);
        }

        public void Set<T>(string propertyName, T propertyValue)
        {
            properties[propertyName] = propertyValue;
        }

        private Dictionary<string, object> properties = new Dictionary<string, object>();
    }
}