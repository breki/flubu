using Flubu.Tasks.Iis.Iis7;

namespace Flubu.Tasks.Iis
{
    public interface IAddWebsiteBindingTask : ITask
    {
        Iis7AddWebsiteBindingTask AddBinding(string protocol);
        Iis7AddWebsiteBindingTask SiteName(string name);
    }
}