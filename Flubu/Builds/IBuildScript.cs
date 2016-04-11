using System.Collections.Generic;

namespace Flubu.Builds
{
    public interface IBuildScript
    {
        int Run(ICollection<string> args);
    }
}