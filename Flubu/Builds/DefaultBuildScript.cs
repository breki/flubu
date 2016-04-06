using System;
using System.Collections.Generic;
using Flubu.Targeting;

namespace Flubu.Builds
{
    public abstract class DefaultBuildScript : IBuildScript
    {
        protected DefaultBuildScript()
        {
        }

        protected DefaultBuildScript(
            Action<TargetTree> targetBuildingAction,
            Action<TaskSession> buildPropertiesConfiguringAction)
        {
            if (targetBuildingAction == null)
                throw new ArgumentNullException("targetBuildingAction");
            if (buildPropertiesConfiguringAction == null)
                throw new ArgumentNullException("buildPropertiesConfiguringAction");

            this.targetBuildingAction = targetBuildingAction;
            this.buildPropertiesConfiguringAction = buildPropertiesConfiguringAction;
        }

        public Func<bool> InteractiveSessionDetectionFunc
        {
            get { return interactiveSessionDetectionFunc; }
            set { interactiveSessionDetectionFunc = value; }
        }

        public int Run(ICollection<string> args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            TargetTree targetTree = new TargetTree();
            BuildTargets.FillBuildTargets(targetTree);

            if (targetBuildingAction != null)
                targetBuildingAction(targetTree);

            ConfigureTargets(targetTree, args);

            return RunBuild(args, targetTree);
        }

        protected virtual void ConfigureBuildProperties(TaskSession session)
        {
        }

        protected virtual void ConfigureTargets(TargetTree targetTree, ICollection<string> args)
        {
        }

        private int RunBuild (ICollection<string> args, TargetTree targetTree)
        {
            if (targetTree == null)
                throw new ArgumentNullException ("targetTree");

            using (TaskSession session = new TaskSession (new SimpleTaskContextProperties (), args, targetTree))
            {
                session.IsInteractive = InteractiveSessionDetectionFunc ();

                BuildTargets.FillDefaultProperties (session);
                session.Start (BuildTargets.OnBuildFinished);

                session.AddLogger (new MulticoloredConsoleLogger (Console.Out));

                if (buildPropertiesConfiguringAction != null)
                    buildPropertiesConfiguringAction(session);

                ConfigureBuildProperties(session);

                try
                {
                    string targetToRun = ParseCmdLineArgs (args, session);

                    if (targetToRun == null)
                    {
                        ITarget defaultTarget = targetTree.DefaultTarget;
                        if (defaultTarget == null)
                            throw new InvalidOperationException ("The default build target is not defined");

                        targetTree.RunTarget (session, defaultTarget.TargetName);
                    }
                    else
                    {
                        if (false == targetTree.HasTarget (targetToRun))
                        {
                            session.WriteError ("ERROR: The target '{0}' does not exist", targetToRun);
                            targetTree.RunTarget (session, "help");
                            return 2;
                        }

                        targetTree.RunTarget (session, targetToRun);
                    }

                    session.Complete ();

                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine (ex);
                    return 1;
                }
            }
        }

        private static bool DefaultSessionInteractiveSessionDetectionFunc ()
        {
            return Environment.GetEnvironmentVariable ("CI") == null
                   && Environment.GetEnvironmentVariable ("APPVEYOR") == null
                   && Environment.GetEnvironmentVariable ("BUILD_NUMBER") == null;
        }

        private static string ParseCmdLineArgs (IEnumerable<string> args, ITaskContext context)
        {
            string targetToBuild = null;

            foreach (string arg in args)
            {
                if (string.Compare (arg, "-speechdisabled", StringComparison.OrdinalIgnoreCase) == 0)
                    context.Properties.Set (BuildProps.SpeechDisabled, true);
                else
                    targetToBuild = arg;
            }

            return targetToBuild;
        }

        private readonly Action<TargetTree> targetBuildingAction;
        private readonly Action<TaskSession> buildPropertiesConfiguringAction;
        private Func<bool> interactiveSessionDetectionFunc = DefaultSessionInteractiveSessionDetectionFunc;
    }
}
