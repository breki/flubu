using System;
using System.IO;
using Flubu.Tasks.FileSystem;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;

namespace Flubu.Tests
{
    /// <summary>
    /// Tests for <see cref="UnzipFilesTask"/> task.
    /// </summary>
    [TestFixture]
    public class UnzipTests
    {
        private string BaseDirectory
        {
            get { return new FileInfo(GetType().Assembly.Location).DirectoryName; }
        }

        private string SampleArchive
        {
            get { return new FileInfo(Path.Combine(BaseDirectory, @"..\..\SampleArchive.zip")).FullName; }
        }

        private string OutputDirectoryRoot
        {
            get { return Path.Combine(BaseDirectory, @"Extracted"); }
        }

        private string OutputDirectory
        {
            get { return Path.Combine(BaseDirectory, @"Extracted\content"); }
        }

        /// <summary>
        /// Clean output directory.
        /// </summary>
        [SetUp]
        public void Clean()
        {
            string dir = OutputDirectoryRoot;
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Unzip local file.
        /// </summary>
        [Test]
        public void Extractfile()
        {
            TestUnzipTask(new UnzipFilesTask(SampleArchive, OutputDirectory));
        }

        /// <summary>
        /// Unzip unseekable stream.
        /// </summary>
        [Test]
        public void ExtractNonSeekableStream()
        {
            using (
                FileStream f = new FileStream(@"..\..\SampleArchive.zip", FileMode.Open, FileAccess.Read, FileShare.Read)
                )
            {
                TestUnzipTask(new UnzipFilesTask(new UnseekableStream(f), OutputDirectory));
            }
        }

        /// <summary>
        /// Verify that <see cref="FastZip"/> still does not support unseekable streams.
        /// </summary>
        /// <remarks>
        /// Once <see cref="FastZip"/> supports unseekable streams use it to extract streams
        /// rather than to provide custom extraction procedure for streams in <see cref="UnzipFilesTask"/>.
        /// </remarks>
        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Stream is not seekable", MatchType = MessageMatch.Contains)]
        public void SharpZipLibUnseekableStreamsupport()
        {
            FastZip zip = new FastZip();
            FileStream f = new FileStream(@"..\..\SampleArchive.zip", FileMode.Open, FileAccess.Read, FileShare.Read);
            zip.ExtractZip(
                new UnseekableStream(f), OutputDirectory, FastZip.Overwrite.Always, null, null, null, true, true);
        }

        private void TestUnzipTask(UnzipFilesTask unzip)
        {
            unzip.FileFilter = "t";
            unzip.DirectoryFilter = @"-Debug";
            ITaskContext c = new TaskContext (new SimpleTaskContextProperties (), new string[0]);
            unzip.Execute(c);
            TestExtractedFiles();
        }

        private void TestExtractedFiles()
        {
            string dir = OutputDirectory;
            foreach (string file in new[]
                                        {
                                            @"Flubu/Builds/VSSolutionBrowsing/VSProjectConfiguration.cs",
                                            @"bin/Release/Flubu.Tests.dll",
                                            @"obj/Release/Flubu.Tests.pdb",
                                        })
            {
                Assert.IsTrue(File.Exists(Path.Combine(dir, file)), "Exists:" + file);
            }
            foreach (string file in new[]
                                        {
                                            @"Flubu/Builds/HudsonHelper.cs",
                                            @"bin/Debug/Flubu.Tests.dll",
                                            @"obj/Debug/Flubu.Tests.pdb",
                                        })
            {
                Assert.IsFalse(File.Exists(Path.Combine(dir, file)), "Exists:" + file);
            }
        }


        private class UnseekableStream : Stream
        {
            private readonly Stream mStream;

            public UnseekableStream(Stream stream)
            {
                mStream = stream;
            }

            public override void Flush()
            {
                mStream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return mStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                mStream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return mStream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                mStream.Write(buffer, offset, count);
            }

            public override bool CanRead
            {
                get { return mStream.CanRead; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return mStream.CanWrite; }
            }

            public override long Length
            {
                get { return mStream.Length; }
            }

            public override long Position
            {
                get { return mStream.Position; }
                set { throw new NotImplementedException("Just testing."); }
            }
        }
    }
}