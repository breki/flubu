namespace Flubu.Packaging
{
    public class CopyProcessorTransformation
    {
        public CopyProcessorTransformation(
            string sourceId, LocalPath destinationPath, CopyProcessorTransformationOptions options)
        {
            this.sourceId = sourceId;
            this.destinationPath = destinationPath;
            this.options = options;
        }

        public string SourceId
        {
            get { return sourceId; }
        }

        public LocalPath DestinationPath
        {
            get { return destinationPath; }
        }

        public CopyProcessorTransformationOptions Options
        {
            get { return options; }
        }

        private readonly string sourceId;
        private readonly LocalPath destinationPath;
        private readonly CopyProcessorTransformationOptions options = CopyProcessorTransformationOptions.None;
    }
}