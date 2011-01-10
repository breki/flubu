using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Flubu.Packaging
{
    public class CopyProcessor : IPackageProcessor
    {
        public CopyProcessor(ITaskContext taskContext, ICopier copier, string destinationRootDir)
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

        private ICompositeFilesSource ProcessPrivate(
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

                LocalPath destinationPath = FindDestinationPathForSource(filesSource.Id);

                foreach (PackagedFileInfo sourceFile in filesSource.ListFiles())
                {
                    if (false == LoggingHelper.LogIfFilteredOut(sourceFile.FileFullPath.ToString(), filter, taskContext))
                        continue;

                    FullPath destinationFullPath = new FullPath(destinationRootDir);
                    destinationFullPath = destinationFullPath.CombineWith(destinationPath);

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

                transformedCompositeSource.AddFilesSource(filesList);
            }

            return transformedCompositeSource;
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
        private readonly string destinationRootDir;
        private readonly Dictionary<string, LocalPath> transformations = new Dictionary<string, LocalPath>();
        private readonly Dictionary<string, string> fileTransformations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private IFileFilter filter;
    }
}