using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Flubu.Tasks.FileSystem
{
    public class ZipFilesTask : TaskBase
    {
        public ZipFilesTask (string zipFileName, string baseDir, IEnumerable<string> filesToZip)
        {
            this.zipFileName = zipFileName;
            this.baseDir = baseDir;
            this.filesToZip.AddRange (filesToZip);

            Path.GetExtension (zipFileName);
        }

        public int? CompressionLevel
        {
            get { return compressionLevel; }
            set { compressionLevel = value; }
        }

        public override string Description
        {
            get
            {
                return String.Format (
                    CultureInfo.InvariantCulture,
                    "Zipping {1} files to the '{0}' archive, using the base directory '{2}'",
                    zipFileName,
                    filesToZip.Count,
                    baseDir);
            }
        }

        public Func<string, string> ZipFileFooterCallback
        {
            get { return zipFileFooterCallback; }
            set { zipFileFooterCallback = value; }
        }

        public Func<string, string> ZipFileHeaderCallback
        {
            get { return zipFileHeaderCallback; }
            set { zipFileHeaderCallback = value; }
        }

        public Func<string, string> FileNameRewriteFunc
        {
            get { return fileNameRewriteFunc; }
            set { fileNameRewriteFunc = value; }
        }

        public Predicate<string> FileFilterFunc
        {
            get { return fileFilterFunc; }
            set { fileFilterFunc = value; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            using (FileStream zipFileStream = new FileStream (
                zipFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream (zipFileStream))
                {
                    if (compressionLevel.HasValue)
                        zipStream.SetLevel(compressionLevel.Value);

                    buffer = new byte[1024 * 1024];

                    foreach (string fileName in filesToZip)
                    {
                        if (!fileFilterFunc(fileName))
                        {
                            context.WriteInfo("Skipping file '{0}'", fileName);
                            continue;
                        }

                        int skipChar = 0;

                        if (false == String.IsNullOrEmpty (baseDir)
                            && (baseDir[baseDir.Length - 1] == '\\'
                            || baseDir[baseDir.Length - 1] == '/'))
                            skipChar++;

                        // cut off the leading part of the path (up to the root directory of the package)
                        string basedFileName = fileName.Substring (baseDir.Length + skipChar);

                        basedFileName = fileNameRewriteFunc(basedFileName);

                        basedFileName = ZipEntry.CleanName (basedFileName);

                        context.WriteInfo("Zipping file '{0}'", basedFileName);
                        AddFileToZip(fileName, basedFileName, zipStream);
                    }
                }
            }
        }

        private void AddFileToZip(string fileName, string basedFileName, ZipOutputStream zipStream)
        {
            using (FileStream fileStream = File.OpenRead (fileName))
            {
                string fileHeader = String.Empty;
                string fileFooter = String.Empty;

                if (zipFileHeaderCallback != null)
                    fileHeader = zipFileHeaderCallback (fileName);

                if (zipFileFooterCallback != null)
                    fileFooter = zipFileFooterCallback (fileName);

                ZipEntry entry = new ZipEntry (basedFileName);
                entry.DateTime = File.GetLastWriteTime (fileName);
                entry.Size = fileStream.Length + fileHeader.Length + fileFooter.Length;
                zipStream.PutNextEntry (entry);

                int sourceBytes;

                WriteTextToZipStream(fileHeader, zipStream);

                while (true)
                {
                    sourceBytes = fileStream.Read (buffer, 0, buffer.Length);

                    if (sourceBytes == 0)
                        break;

                    zipStream.Write (buffer, 0, sourceBytes);
                }

                WriteTextToZipStream (fileFooter, zipStream);
            }
        }

        private static void WriteTextToZipStream(string text, ZipOutputStream zipStream)
        {
            if (text.Length > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes (text);
                zipStream.Write (bytes, 0, bytes.Length);                    
            }
        }

        private readonly string baseDir;
        private byte[] buffer;
        private readonly List<string> filesToZip = new List<string> ();
        private int? compressionLevel;
        private Func<string, string> zipFileFooterCallback;
        private Func<string, string> zipFileHeaderCallback;
        private Func<string, string> fileNameRewriteFunc = x => x;
        private Predicate<string> fileFilterFunc = x => true;
        private readonly string zipFileName;
    }
}