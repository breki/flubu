using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Flubu.Services
{
    public interface IProcessRunner
    {
        int Run (
            [NotNull]
            string programExePath,
            [CanBeNull]
            string arguments,
            [CanBeNull]
            string workingDirectory,
            [CanBeNull]
            TimeSpan? executionTimeout,
            [CanBeNull]
            DataReceivedEventHandler outputDataReceived,
            [CanBeNull]
            DataReceivedEventHandler errorDataReceived);
    }
}