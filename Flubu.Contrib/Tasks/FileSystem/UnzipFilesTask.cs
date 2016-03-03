using System;
using System.Globalization;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Flubu.Tasks.FileSystem
{
    /// <summary>
    /// Extracts content of ZIP archive to local file system.
    /// </summary>
    public class UnzipFilesTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnzipFilesTask"/> class for extraction from local file.
        /// </summary>
        /// <param name="zipFileName">Name of local file (ZIP archive) to extract.</param>
        /// <param name="destinationDirectory">Directory to which files will be extracted.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads")]
        public UnzipFilesTask(string zipFileName, string destinationDirectory)
        {
            if (zipFileName == null) 
                throw new ArgumentNullException("zipFileName");
            if (destinationDirectory == null) 
                throw new ArgumentNullException("destinationDirectory");

            this.zipFileName = zipFileName;
            this.destinationDirectory = destinationDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnzipFilesTask"/> class for extraction from HTTP URL.
        /// </summary>
        /// <param name="url">URL for ZIP archive.</param>
        /// <param name="destinationDirectory">Directory to which files will be extracted.</param>
        public UnzipFilesTask(Uri url, string destinationDirectory)
        {
            if (url == null) 
                throw new ArgumentNullException("url");
            if (destinationDirectory == null) 
                throw new ArgumentNullException("destinationDirectory");

            this.url = url;
            this.destinationDirectory = destinationDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnzipFilesTask"/> class for extraction from <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Initialized input stream. Stream is not closed by this task.</param>
        /// <param name="destinationDirectory">Directory to which files will be extracted.</param>
        public UnzipFilesTask(Stream stream, string destinationDirectory)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (destinationDirectory == null) throw new ArgumentNullException("destinationDirectory");
            this.stream = stream;
            this.destinationDirectory = destinationDirectory;
        }

        public static void Execute(ITaskContext context, string zipFileName, string destinationDirectory)
        {
            new UnzipFilesTask(zipFileName, destinationDirectory).Execute(context);
        }

        /// <summary>
        /// Gets or sets file name filter for extracted files.
        /// </summary>
        /// <remarks>
        /// This is <a href="http://wiki.sharpdevelop.net/SharpZipLib_FastZip.ashx?HL=namefilter#How_to_extract_a_Zip_File_using_FastZip_3">FastZip NameFilter</a>.
        /// Basically it is semicolon separated list of regular expressions. If expression is prefixed by - it is treated as exclusion filter.
        /// </remarks>
        public string FileFilter { get; set; }

        /// <summary>
        /// Gets or sets directory filter for extracted files.
        /// </summary>
        /// <remarks>
        /// This is <a href="http://wiki.sharpdevelop.net/SharpZipLib_FastZip.ashx?HL=namefilter#How_to_extract_a_Zip_File_using_FastZip_3">FastZip NameFilter</a>.
        /// Basically it is semicolon separated list of regular expressions. If expression is prefixed by - it is treated as exclusion filter.
        /// </remarks>
        public string DirectoryFilter { get; set; }

        public override string Description
        {
            get
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Unzipping '{0}' to '{1}'",
                    zipFileName ?? (object)url ?? "<stream>",
                    destinationDirectory);

                return message;
            }
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (zipFileName != null)
            {
                FastZip fastZip = new FastZip();
                fastZip.CreateEmptyDirectories = true;
                fastZip.ExtractZip(
                    zipFileName,
                    destinationDirectory,
                    FastZip.Overwrite.Always,
                    null,
                    FileFilter,
                    DirectoryFilter,
                    true);
            }
            else if (url != null)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (WebResponse response = request.GetResponse())
                    Extract(response.GetResponseStream());
            }
            else if (stream != null)
            {
                Extract(stream);
            }
            else
            {
                throw new NotSupportedException("Unsupported archive source.");
            }
        }

        private readonly string zipFileName;
        private readonly Uri url;
        private readonly Stream stream;
        private readonly string destinationDirectory;

        private void Extract(Stream forwardStream)
        {
            NameFilter fileFilter = new NameFilter(FileFilter);
            NameFilter directoryFilter = new NameFilter(DirectoryFilter);
            using (ZipInputStream zip = new ZipInputStream(forwardStream))
            {
                for (ZipEntry entry = zip.GetNextEntry(); entry != null; entry = zip.GetNextEntry())
                {
                    if (entry.IsFile)
                    {
                        if (directoryFilter.IsMatch(Path.GetDirectoryName(entry.Name)) && fileFilter.IsMatch(entry.Name))
                        {
                            ExtractFile(entry, zip);
                        }
                    }
                    else if (entry.IsDirectory)
                    {
                        if (directoryFilter.IsMatch(entry.Name))
                        {
                            ExtractDirectory(entry);
                        }
                    }
                    else
                    {
                        // TODO: What to do with other types?
                    }
                }
            }
        }

        private void ExtractDirectory(ZipEntry zipEntry)
        {
            string filePath = Path.Combine(destinationDirectory, zipEntry.Name);
            Directory.CreateDirectory(filePath);
        }

        private void ExtractFile(ZipEntry zipEntry, ZipInputStream zip)
        {
            string filePath = Path.Combine(destinationDirectory, zipEntry.Name);
            FileInfo i = new FileInfo(filePath);
            i.Directory.Create();
            using (FileStream file = File.Create(filePath))
            {
                byte[] buffer = new byte[0x4000];
                int n;
                do
                {
                    n = zip.Read(buffer, 0, buffer.Length);
                    if (n > 0)
                    {
                        file.Write(buffer, 0, n);
                    }
                } 
                while (n == buffer.Length);
            }

            File.SetLastWriteTime(filePath, zipEntry.DateTime);
        }
    }
}