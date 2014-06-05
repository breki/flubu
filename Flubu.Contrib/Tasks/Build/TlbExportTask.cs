using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Flubu.Tasks.Processes;

namespace Flubu.Tasks.Build
{
    public class TlbExportTask : TaskBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TlbExportTask"/> class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to export type library.</param>
        public TlbExportTask(string assemblyName)
        {
            WorkingDirectory = ".";
            ExecutablePath = "tools\\tlb\\tlbexp.exe";
            AssemblyName = assemblyName;
        }

        /// <summary>
        ///   Gets or sets documentation working directory. (default=.)
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        ///   Gets or sets executable path (tlbexp.exe). (default=tools\\tlb\\tlbexp.exe)
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public string ExecutablePath { get; set; }

        public string AssemblyName { get; set; }

        public override string Description
        {
            get { return string.Format(CultureInfo.InvariantCulture, "Export type library for assembly {0}", AssemblyName); }
        }

        public TlbExportTask SetWorkingDirectory(string fullPath)
        {
            WorkingDirectory = fullPath;
            return this;
        }

        public TlbExportTask SetExecutablePath(string fullPath)
        {
            ExecutablePath = fullPath;
            return this;
        }

        protected override void DoExecute(ITaskContext context)
        {
            RunProgramTask task = new RunProgramTask(ExecutablePath)
                .SetWorkingDir(WorkingDirectory);

            task.AddArgument(AssemblyName)
                .Execute(context);
        }
    }
}