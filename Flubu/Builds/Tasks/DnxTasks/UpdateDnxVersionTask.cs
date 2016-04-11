using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Flubu.Tasks.Processes;

namespace Flubu.Builds.Tasks.DnxTasks
{
    public class UpdateDnxVersionTask : TaskBase
    {
        public UpdateDnxVersionTask(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; set; }

        public override string Description
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return string.Format(CultureInfo.InvariantCulture, "Updating version on file {0}", FileName); }
        }

        public static UpdateDnxVersionTask Create(string fileName)
        {
            return new UpdateDnxVersionTask(fileName);
        } 

        protected override void DoExecute(ITaskContext context)
        {
            Version version = context.Properties.Get<Version>(BuildProps.BuildVersion);

            List<string> lines = new List<string>(File.ReadAllLines(Path.GetFullPath(FileName)));
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                if (line.IndexOf("\"version\":", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    lines[i] = string.Format(CultureInfo.InvariantCulture, "  \"version\": \"{0}.{1}.{2}-*\",", version.Major, version.Minor, version.Revision);
                    break;
                }
            }

            File.WriteAllLines(FileName, lines);
        }
    }
}
