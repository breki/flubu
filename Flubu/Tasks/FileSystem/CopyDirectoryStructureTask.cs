using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;

namespace Flubu.Tasks.FileSystem
{
    /// <summary>
    /// Copies a directory tree from the source to the destination.
    /// </summary>
    public class CopyDirectoryStructureTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDirectoryStructureTask"/> class
        /// using a specified source and destination path and an indicator whether to overwrite existing files.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="overwriteExisting">if set to <c>true</c> the task will overwrite existing destination files.</param>
        public CopyDirectoryStructureTask (string sourcePath, string destinationPath, bool overwriteExisting)
        {
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
            this.overwriteExisting = overwriteExisting;
        }

        /// <summary>
        /// Gets the list of all destination files that were copied.
        /// </summary>
        /// <value>The list of all destination files that were copied.</value>
        public IList<string> CopiedFilesList
        {
            get { return copiedFilesList; }
        }

        /// <summary>
        /// Gets or sets the exclusion regular expression pattern for files.
        /// </summary>
        /// <remarks>All files whose paths match this regular expression
        /// will not be copied. If the <see cref="ExclusionPattern"/> is <c>null</c>, it will be ignored.</remarks>
        /// <value>The exclusion pattern.</value>
        public string ExclusionPattern
        {
            get { return exclusionPattern; }
            set { exclusionPattern = value; }
        }

        /// <summary>
        /// Gets or sets the inclusion regular expression pattern for files.
        /// </summary>
        /// <remarks>All files whose paths match this regular expression
        /// will be copied. If the <see cref="InclusionPattern"/> is <c>null</c>, it will be ignored.</remarks>
        /// <value>The inclusion pattern.</value>
        public string InclusionPattern
        {
            get { return inclusionPattern; }
            set { inclusionPattern = value; }
        }

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
                    "Copy directory structure from '{0}' to '{1}", 
                    sourcePath, 
                    destinationPath);
            }
        }

        /// <summary>
        /// Copies a directory tree from the source to the destination.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="overwriteExisting">if set to <c>true</c> the task will overwrite existing destination files.</param>
        public static void Execute(
            ITaskContext context, 
            string sourcePath, 
            string destinationPath, 
            bool overwriteExisting)
        {
            CopyDirectoryStructureTask task = new CopyDirectoryStructureTask (sourcePath, destinationPath, overwriteExisting);
            task.Execute (context);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            copiedFilesList = new List<string>();

            Regex inclusionRegex = null;
            if (inclusionPattern != null)
                inclusionRegex = new Regex(inclusionPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Regex exclusionRegex = null;
            if (exclusionPattern != null)
                exclusionRegex = new Regex (exclusionPattern, RegexOptions.IgnoreCase|RegexOptions.Singleline);

            CopyStructureRecursive (context, sourcePath, destinationPath, inclusionRegex, exclusionRegex);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void CopyStructureRecursive(
            ITaskContext context, 
            string sourcePathRecursive, 
            string destinationPathRecursive, 
            Regex inclusionRegex, 
            Regex exclusionRegex)
        {
            if (exclusionRegex != null && exclusionRegex.IsMatch (sourcePathRecursive))
                return;

            DirectoryInfo info = new DirectoryInfo (sourcePathRecursive);

            foreach (FileSystemInfo fileSystemInfo in info.GetFileSystemInfos ())
            {
                if (fileSystemInfo is FileInfo)
                {
                    if (inclusionRegex != null && false == inclusionRegex.IsMatch(fileSystemInfo.FullName))
                        continue;
                    if (exclusionRegex != null && exclusionRegex.IsMatch(fileSystemInfo.FullName))
                        continue;

                    FileInfo fileInfo = fileSystemInfo as FileInfo;
                    string filePath = Path.Combine (destinationPathRecursive, fileInfo.Name);

                    if (false == Directory.Exists(destinationPathRecursive))
                        Directory.CreateDirectory(destinationPathRecursive);

                    fileInfo.CopyTo (filePath, overwriteExisting);
                    context.WriteInfo(
                        "Copied file '{0}' to '{1}'", 
                        fileSystemInfo.FullName, 
                        filePath);
                    copiedFilesList.Add(filePath);
                }
                else
                {
                    DirectoryInfo dirInfo = fileSystemInfo as DirectoryInfo;
                    string subdirectoryPath = Path.Combine (
                        destinationPathRecursive, 
                        // ReSharper disable once PossibleNullReferenceException
                        dirInfo.Name);
                    CopyStructureRecursive (
                        context, 
                        dirInfo.FullName, 
                        subdirectoryPath, 
                        inclusionRegex, 
                        exclusionRegex);
                }
            }
        }

        private readonly string destinationPath;
        private string exclusionPattern;
        private string inclusionPattern;
        private readonly bool overwriteExisting;
        private readonly string sourcePath;
        private List<string> copiedFilesList;
    }
}
