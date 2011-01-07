namespace Flubu.Packaging
{
    public class NegativeFilter : IFileFilter
    {
        public NegativeFilter(IFileFilter filter)
        {
            this.filter = filter;
        }

        public bool IsPassedThrough(string fileName)
        {
            return !filter.IsPassedThrough(fileName);
        }

        private readonly IFileFilter filter;
    }
}