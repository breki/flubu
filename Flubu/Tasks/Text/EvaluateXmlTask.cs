using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.XPath;

namespace Flubu.Tasks.Text
{
    /// <summary>
    /// Evaluates XPath expressions on a specified XML file and stores results in <seealso cref="ITaskContext"/> properties.
    /// </summary>
    public class EvaluateXmlTask : TaskBase
    {
        public EvaluateXmlTask (string xmlFileName)
        {
            this.xmlFileName = xmlFileName;
        }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return String.Format (
                    CultureInfo.InvariantCulture, 
                    "Evaluate {0} XPath expressions on file '{1}'.", 
                    expressions.Count, 
                    xmlFileName);
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

        public EvaluateXmlTask AddExpression(string propertyName, string xpath)
        {
            expressions.Add(new Expression(propertyName, xpath));
            return this;
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            using (StreamReader reader = new StreamReader(xmlFileName))
            {
                XPathDocument doc = new XPathDocument(reader);
                XPathNavigator navigator = doc.CreateNavigator();

                foreach (Expression expression in expressions)
                {
                    object result = navigator.Evaluate (expression.Xpath);
                    context.Properties.Set (expression.PropertyName, result);

                    context.WriteDebug (
                        "Property '{1}': executing XPath expression '{0}' evaluates to '{2}'", 
                        expression.Xpath, 
                        expression.PropertyName, 
                        result);
                }
            }
        }

        private readonly string xmlFileName;
        private readonly List<Expression> expressions = new List<Expression>();

        private class Expression
        {
            public Expression(string propertyName, string xpath)
            {
                this.xpath = xpath;
                this.propertyName = propertyName;
            }

            public string Xpath
            {
                get { return xpath; }
            }

            public string PropertyName
            {
                get { return propertyName; }
            }

            private readonly string xpath;
            private readonly string propertyName;            
        }
    }
}