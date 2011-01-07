using System;
using System.Collections.Generic;
using System.Diagnostics;
using Flubu.Targeting;

namespace Flubu
{
    public interface IFlubuRunner : IDisposable
    {
        Stopwatch BuildStopwatch { get; }

        bool HasFailed { get; }

        IDictionary<string, ITarget> Targets { get; }
    }
}