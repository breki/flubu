using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Flubu.Packaging
{
    public class Zipper : IZipper
    {
        public Zipper(ILogger logger)
        {
            this.logger = logger;
        }

        public void ZipFiles(
            string zipFileName, 
            string baseDir,
            int? compressionLevel,
            IEnumerable<string> filesToZip)
        {
            using (FileStream zipFileStream = new FileStream(
                zipFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None))
            {
                using (ZipOutputStream zipStream = new ZipOutputStream(zipFileStream))
                {
                    if (compressionLevel.HasValue)
                        zipStream.SetLevel(compressionLevel.Value);

                    buffer = new byte[1024 * 1024];

                    foreach (string fileName in filesToZip)
                    {
                        int skipChar = 0;

                        if (false == String.IsNullOrEmpty(baseDir)
                            && (baseDir[baseDir.Length - 1] == '\\'
                            || baseDir[baseDir.Length - 1] == '/'))
                            skipChar++;

                        // cut off the leading part of the path (up to the root directory of the package)
                        string basedFileName = fileName.Substring(baseDir.Length + skipChar);

                        basedFileName = ZipEntry.CleanName(basedFileName);

                        //environment.LogMessage("Zipping file '{0}'", basedFileName);
                        AddFileToZip(fileName, basedFileName, zipStream);
                    }
                }
            }
        }

        private void AddFileToZip(string fileName, string basedFileName, ZipOutputStream zipStream)
        {
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                string fileHeader = String.Empty;
                string fileFooter = String.Empty;

                //if (zipFileHeaderCallback != null)
                //    fileHeader = zipFileHeaderCallback(fileName);

                //if (zipFileFooterCallback != null)
                //    fileFooter = zipFileFooterCallback(fileName);

                ZipEntry entry = new ZipEntry(basedFileName);
                entry.DateTime = File.GetLastWriteTime(fileName);
                entry.Size = fileStream.Length + fileHeader.Length + fileFooter.Length;
                zipStream.PutNextEntry(entry);

                int sourceBytes;

                WriteTextToZipStream(fileHeader, zipStream);

                while (true)
                {
                    sourceBytes = fileStream.Read(buffer, 0, buffer.Length);

                    if (sourceBytes == 0)
                        break;

                    zipStream.Write(buffer, 0, sourceBytes);
                }

                WriteTextToZipStream(fileFooter, zipStream);
            }
        }

        private static void WriteTextToZipStream(string text, ZipOutputStream zipStream)
        {
            if (text.Length > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text.ToString());
                zipStream.Write(bytes, 0, bytes.Length);
            }
        }

        private byte[] buffer;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly ILogger logger;
    }
}