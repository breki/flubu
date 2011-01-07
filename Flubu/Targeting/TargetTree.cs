﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace Flubu.Targeting
{
    public class TargetTree
    {
        public TargetTree()
        {
            AddTarget("help")
                .SetDescription("Displays the available targets in the build")
                .Do(TargetHelp);            
        }
        
        /// <summary>
        /// Gets the default target for this runner.
        /// </summary>
        /// <remarks>The default target is the one which will be executed if
        /// the target is not specified in the command line.</remarks>
        /// <value>The default target.</value>
        public ITarget DefaultTarget
        {
            get { return defaultTarget; }
        }

        public ITarget AddTarget(string targetName)
        {
            ITarget target = new Target(this, targetName);
            targets.Add(target.TargetName, target);
            return target;
        }

        public void EnsureDependenciesExecuted(ITaskContext taskContext, string targetName)
        {
            ITarget target = targets[targetName];
            foreach (string dependency in target.Dependencies)
            {
                if (false == executedTargets.Contains(dependency))
                {
                    RunTarget(taskContext, dependency);
                }
            }
        }

        public ITarget GetTarget(string targetName)
        {
            return targets[targetName];
        }

        /// <summary>
        /// Determines whether the specified target exists.
        /// </summary>
        /// <param name="targetName">Name of the target.</param>
        /// <returns>
        ///     <c>true</c> if the specified target exists; otherwise, <c>false</c>.
        /// </returns>
        public bool HasTarget(string targetName)
        {
            return targets.ContainsKey(targetName);
        }

        public void MarkTargetAsExecuted(ITarget target)
        {
            executedTargets.Add(target.TargetName);
        }

        public void RunTarget(ITaskContext taskContext, string targetName)
        {
            if (false == targets.ContainsKey(targetName))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "The target '{0}' does not exist", 
                    targetName);
    
                throw new ArgumentException(message);
            }

            ITarget target = targets[targetName];
            target.Execute(taskContext);
        }

        public void SetDefaultTarget(ITarget target)
        {
            this.defaultTarget = target;
        }

        /// <summary>
        /// The target for displaying help in the command line.
        /// </summary>
        /// <param name="context">The task context.</param>
        public void TargetHelp(ITaskContext context)
        {
            context.WriteInfo("Targets:");

            // first sort the targets
            SortedList<string, ITarget> sortedTargets = new SortedList<string, ITarget>();

            foreach (ITarget target in targets.Values)
                sortedTargets.Add(target.TargetName, target);

            // now display them in sorted order
            foreach (ITarget target in sortedTargets.Values)
                if (false == target.IsHidden)
                    context.WriteInfo(
                        "  {0} : {1}",
                        target.TargetName,
                        target.Description);
        }

        private ITarget defaultTarget;
        private readonly HashSet<string> executedTargets = new HashSet<string>();
        private readonly Dictionary<string, ITarget> targets = new Dictionary<string, ITarget>();
    }
}