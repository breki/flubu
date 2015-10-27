using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flubu.Tasks.Text
{
    public class ReplaceTokensTask : TaskBase
    {
        public ReplaceTokensTask (
            string sourceFileName,
            string destinationFileName)
        {
            this.sourceFileName = sourceFileName;
            this.destinationFileName = destinationFileName;
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
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Replace tokens in file '{0}' to file '{1}'",
                    sourceFileName,
                    destinationFileName);
            }
        }

        public Encoding SourceFileEncoding
        {
            get { return sourceFileEncoding; }
            set { sourceFileEncoding = value; }
        }

        public Encoding DestionationFileEncoding
        {
            get { return destionationFileEncoding; }
            set { destionationFileEncoding = value; }
        }

        public void AddTokenValue (string token, string value)
        {
            tokens.Add (token, value);
        }

        protected override void DoExecute (ITaskContext context)
        {
            string tokenizedContent = File.ReadAllText(sourceFileName, sourceFileEncoding);

            string finalContent = ReplaceTokens(tokenizedContent);

            File.WriteAllText(destinationFileName, finalContent, destionationFileEncoding);
        }

        private string ReplaceTokens(string tokenizedContent)
        {
            foreach (KeyValuePair<string, string> entry in tokens)
                tokenizedContent = tokenizedContent.Replace("$" + entry.Key + "$", entry.Value);

            return tokenizedContent;
        }

        private readonly string sourceFileName;
        private Encoding sourceFileEncoding = Encoding.UTF8;
        private readonly string destinationFileName;
        private Encoding destionationFileEncoding = Encoding.UTF8;
        private readonly Dictionary<string, string> tokens = new Dictionary<string, string>();
    }
}