using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Flubu.Packaging
{
    public class CopyProcessor : IPackageProcessor
    {
        public CopyProcessor(ITaskContext taskContext, ICopier copier, FullPath destinationRootDir)
        {
            this.taskContext = taskContext;
            this.copier = copier;
            this.destinationRootDir = destinationRootDir;
        }

        public CopyProcessor AddTransformation(string sourceId, LocalPath destinationDir)
        {
            transformations.Add(sourceId, destinationDir);
            return this;
        }

        /// <summary>
        /// Defines a transformation for <see cref="SingleFileSource"/> which copies the file to the destination
        /// and renames the file in the process.
        /// </summary>
        /// <param name="sourceId">ID of the <see cref="SingleFileSource"/>.</param>
        /// <param name="destinationFileName">The destination directory and file name (local path).</param>
        /// <returns>This same instance of the <see cref="CopyProcessor"/>.</returns>
        public CopyProcessor AddSingleFileTransformation (string sourceId, LocalPath destinationFileName)
        {
            singleFileTransformations.Add (sourceId, destinationFileName);
            return this;
        }

        /// <summary>
        /// Replace all occurences of source filename with newFileName.
        /// </summary>
        /// <param name="fileName">Source file name to replace.</param>
        /// <param name="newFileName">Replace with new name.</param>
        /// <returns>Returns <see cref="CopyProcessor"/>.</returns>
        public CopyProcessor AddFileTransformation(string fileName, string newFileName)
        {
            fileTransformations.Add(fileName, newFileName);
            return this;
        }

        public IPackageDef Process(IPackageDef packageDef)
        {
            return (IPackageDef)ProcessPrivate(packageDef, true);
        }

        public void SetFilter(IFileFilter filter)
        {
            this.filter = filter;
        }

        [SuppressMessage ("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private ICompositeFilesSource ProcessPrivate (
            ICompositeFilesSource compositeFilesSource, 
            bool isRoot)
        {
            CompositeFilesSource transformedCompositeSource = isRoot
                ? new StandardPackageDef(compositeFilesSource.Id)
                : new CompositeFilesSource(compositeFilesSource.Id);

            foreach (IFilesSource filesSource in compositeFilesSource.ListChildSources())
            {
                if (filesSource is ICompositeFilesSource)
                    throw new NotImplementedException("Child composites are currently not supported");

                FilesList filesList = new FilesList(filesSource.Id);

                bool transformed = false;
                if (filesSource is SingleFileSource)
                    transformed = TryToTransformSingleFileSource((SingleFileSource)filesSource, filesList);

                if (!transformed)
                    TransformSource(filesSource, filesList);

                transformedCompositeSource.AddFilesSource(filesList);
            }

            return transformedCompositeSource;
        }

        private bool TryToTransformSingleFileSource(SingleFileSource source, FilesList filesList)
        {
            if (!HasSingleFileSourceTransformation (source.Id))
                return false;

            LocalPath destinationPath = FindDestinationPathForSingleFileSource (source.Id);

            PackagedFileInfo sourceFile = source.ListFiles().AsQueryable().First();
            FullPath destinationFullPath = destinationRootDir.CombineWith (destinationPath);
            FileFullPath destinationFileFullPath = destinationFullPath.ToFileFullPath ();

            filesList.AddFile (new PackagedFileInfo (destinationFileFullPath));
            copier.Copy (sourceFile.FileFullPath, destinationFileFullPath);

            return true;
        }

        private bool HasSingleFileSourceTransformation (string sourceId)
        {
            return singleFileTransformations.ContainsKey(sourceId);
        }

        private LocalPath FindDestinationPathForSingleFileSource(string sourceId)
        {
            return singleFileTransformations[sourceId];
        }

        private void TransformSource(IFilesSource filesSource, FilesList filesList)
        {
            LocalPath destinationPath = FindDestinationPathForSource(filesSource.Id);

            foreach (PackagedFileInfo sourceFile in filesSource.ListFiles())
            {
                if (false ==
                    LoggingHelper.LogIfFilteredOut(sourceFile.FileFullPath.ToString(), filter, taskContext))
                    continue;

                FullPath destinationFullPath = destinationRootDir.CombineWith(destinationPath);

                if (sourceFile.LocalPath != null)
                    destinationFullPath = destinationFullPath.CombineWith(sourceFile.LocalPath);
                else
                {
                    destinationFullPath =
                        destinationFullPath.CombineWith(new LocalPath(sourceFile.FileFullPath.FileName));
                }

                string destFile = destinationFullPath.FileName;
                if (fileTransformations.ContainsKey(destFile))
                {
                    destinationFullPath = destinationFullPath.ParentPath.CombineWith(
                        fileTransformations[destFile]);
                }

                FileFullPath destinationFileFullPath = destinationFullPath.ToFileFullPath();
                filesList.AddFile(new PackagedFileInfo(destinationFileFullPath));

                copier.Copy(sourceFile.FileFullPath, destinationFileFullPath);
            }
        }

        private LocalPath FindDestinationPathForSource(string sourceId)
        {
            if (false == transformations.ContainsKey(sourceId))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Source '{0}' is not registered for the transformation",
                    sourceId);
                throw new KeyNotFoundException(message);
            }

            return transformations[sourceId];
        }

        private readonly ITaskContext taskContext;
        private readonly ICopier copier;
        private readonly FullPath destinationRootDir;
        private readonly Dictionary<string, LocalPath> transformations = new Dictionary<string, LocalPath>();
        private readonly Dictionary<string, LocalPath> singleFileTransformations = new Dictionary<string, LocalPath>();
        private readonly Dictionary<string, string> fileTransformations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private IFileFilter filter;
    }
}