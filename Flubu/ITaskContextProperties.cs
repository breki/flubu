using System.Diagnostics.CodeAnalysis;

namespace Flubu
{
    public interface ITaskContextProperties
    {
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        T Get<T>(string propertyName);
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        T Get<T>(string propertyName, T defaultValue);
        bool Has(string propertyName);
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
        void Set<T>(string propertyName, T propertyValue);

        string this[string propertyName]
        {
            get;
            set;
        }
    }
}