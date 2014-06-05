using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace Flubu.Tasks.Text
{
    /// <summary>
    /// Transforms XML file using a XSLT stylesheet.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    public class XsltTransformTask : TaskBase
    {
        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get
            {
                return string.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "XSLT transform file '{0}' using XSLT file '{1}'", 
                    inputFile, 
                    xsltFile);
            }
        }

        /// <summary>
        /// Gets or sets the input XML file path.
        /// </summary>
        /// <value>The input file.</value>
        public string InputFile
        {
            get { return inputFile; }
            set { inputFile = value; }
        }

        /// <summary>
        /// Gets or sets the output XML file path.
        /// </summary>
        /// <value>The output file.</value>
        public string OutputFile
        {
            get { return outputFile; }
            set { outputFile = value; }
        }

        /// <summary>
        /// Gets or sets the XSLT file path.
        /// </summary>
        /// <value>The XSLT file.</value>
        public string XsltFile
        {
            get { return xsltFile; }
            set { xsltFile = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltTransformTask"/> class
        /// that will transform a specified input file to a specified output file using the specified XSLT file.
        /// </summary>
        /// <param name="inputFile">The input file path.</param>
        /// <param name="outputFile">The output file path.</param>
        /// <param name="xsltFile">The XSLT file path.</param>
        public XsltTransformTask (string inputFile, string outputFile, string xsltFile)
        {
            this.inputFile = inputFile;
            this.outputFile = outputFile;
            this.xsltFile = xsltFile;
        }

        /// <summary>
        /// Transforms a specified input file to a specified output file using the specified XSLT file.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="inputFile">The input file path.</param>
        /// <param name="outputFile">The output file path.</param>
        /// <param name="xsltFile">The XSLT file path.</param>
        public static void Execute (
            ITaskContext environment,
            string inputFile, 
            string outputFile, 
            string xsltFile)
        {
            XsltTransformTask task = new XsltTransformTask (inputFile, outputFile, xsltFile);
            task.Execute (environment);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            XsltSettings xsltSettings = new XsltSettings (true, true);
            XmlDocument xsltDoc = new XmlDocument ();
            xsltDoc.Load (xsltFile);

            XmlUrlResolver resolver = new XmlUrlResolver ();
            XslCompiledTransform transform = new XslCompiledTransform (true);
            transform.Load (xsltDoc, xsltSettings, resolver);

            using (Stream inputStream = File.Open (inputFile, FileMode.Open, FileAccess.Read))
            {
                XmlReader reader = XmlReader.Create (inputStream);
                using (XmlWriter writer = XmlWriter.Create (outputFile))
                    transform.Transform (reader, writer);
            }
        }

        private string inputFile;
        private string outputFile;
        private string xsltFile;
    }
}
