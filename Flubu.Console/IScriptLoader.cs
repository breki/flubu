using Flubu.Builds;

namespace Flubu.Console
{
    public interface IScriptLoader
    {
        IBuildScript FindAndCreateBuildScriptInstance(string fileName);
    }
}