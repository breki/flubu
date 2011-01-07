namespace Flubu
{
    public interface ITaskContextProperties
    {
        T Get<T>(string propertyName);
        T Get<T>(string propertyName, T defaultValue);
        bool Has(string propertyName);
        void Set<T>(string propertyName, T propertyValue);
    }
}