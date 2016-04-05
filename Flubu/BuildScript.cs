using System;
using System.Collections.Generic;
using System.Linq;
using Flubu.Builds;
using Flubu.Targeting;

namespace Flubu
{
    public interface IBuildScript
    {
        int Execute(ICollection<string> args);
    }

    public abstract class BuildScript : IBuildScript
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int Execute(ICollection<string> args)
        {
            var targetTree = new TargetTree();
            BuildTargets.FillBuildTargets(targetTree);

            targetTree.GetTarget("compile").SetAsDefault();

            ConfigureTargets(targetTree, args);

            using (var session = new TaskSession(new SimpleTaskContextProperties(), args, targetTree))
            {
                BuildTargets.FillDefaultProperties(session);
                session.Start(BuildTargets.OnBuildFinished);

                session.AddLogger(new MulticoloredConsoleLogger(Console.Out));

                ConfigureBuildProperties(session);
                
                try
                {
                    // actual run
                    if (args == null || args.Count <= 0)
                    {
                        targetTree.RunTarget(session, targetTree.DefaultTarget.TargetName);
                    }
                    else
                    {
                        string targetName = args.FirstOrDefault();
                        if (false == targetTree.HasTarget(targetName))
                        {
                            session.WriteError("ERROR: The target '{0}' does not exist", targetName);
                            targetTree.RunTarget(session, "help");
                            return 2;
                        }

                        targetTree.RunTarget(session, targetName);
                    }

                    session.Complete();

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 1;
                }
            }
        }

        [BuildTarget]
        public virtual void Compile()
        {
        }

        protected abstract void ConfigureBuildProperties(TaskSession session);

        protected abstract void ConfigureTargets(TargetTree targetTree, ICollection<string> args);
    }
}