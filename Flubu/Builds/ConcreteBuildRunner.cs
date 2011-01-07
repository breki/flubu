namespace Flubu.Builds
{
    /// <summary>
    /// A non-generic version of the <see cref="BuildRunner{TRunner}"/> class.
    /// </summary>
    public class ConcreteBuildRunner : BuildRunner<ConcreteBuildRunner>
    {
        public ConcreteBuildRunner(string productId)
            : base(productId)
        {
        }
    }
}