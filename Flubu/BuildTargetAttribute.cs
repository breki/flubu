using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flubu
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BuildTargetAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string DependsOn { get; set; }

        public bool IsHidden { get; set; }
    }
}
