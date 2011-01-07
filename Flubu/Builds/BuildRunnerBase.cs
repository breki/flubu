using System.Diagnostics.CodeAnalysis;

namespace Flubu.Builds
{
    /// <summary>
    /// Task Runner helper.
    /// </summary>
    /// <typeparam name="TRunner">Concrete runner <see cref="BuildRunner{TRunner}"/></typeparam>
    public abstract class BuildRunnerBase<TRunner> : BuildRunner<TRunner>
        where TRunner : BuildRunner<TRunner>
    {
        protected BuildRunnerBase(string productId, string[] args) : base(productId)
        {
            Arguments = args;
        }

        /// <summary>
        /// Gets or sets Command line arguments pased to build runner.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] Arguments { get; set; }

        /// <summary>
        /// Method that will configure available build targets.
        /// </summary>
        public abstract void Configure();

        /// <summary>
        /// Target runner.
        /// </summary>
        /// <returns>0 if all ok.</returns>
        public virtual int Run()
        {
            return FlubuRunner<TRunner>.Run(this, Arguments);
        }
    }
}