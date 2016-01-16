using System;
using System.Globalization;
using System.IO;

namespace Flubu.Builds.Tasks.VersioningTasks
{
    public class FetchBuildVersionFromFileTask : TaskBase, IFetchBuildVersionTask
    {
        public FetchBuildVersionFromFileTask(
            string productRootDir,
            string productId)
        {
            this.productRootDir = productRootDir;
            this.productId = productId;
        }

        public override string Description
        {
            get { return "Fetch build version"; }
        }

        public Version BuildVersion
        {
            get { return buildVersion; }
        }

        public string ProjectVersionFileName { get; set; }

        protected override void DoExecute(ITaskContext context)
        {
            string projectVersionFileName;
              
            if (!string.IsNullOrEmpty(ProjectVersionFileName))
            {
                projectVersionFileName = Path.Combine(productRootDir, ProjectVersionFileName);
            }
            else
            {
                projectVersionFileName = Path.Combine(productRootDir, productId + ".ProjectVersion.txt");
            }

            if (false == File.Exists(projectVersionFileName))
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Project version file ('{0}') is missing.", 
                    projectVersionFileName);
                throw new InvalidOperationException(message);
            }

            using (Stream stream = File.Open(projectVersionFileName, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string versionAsString = reader.ReadLine();
                    buildVersion = new Version(versionAsString);
                }
            }

            context.WriteInfo("Project build version (from file): {0}", buildVersion);
        }

        private Version buildVersion;
        private readonly string productRootDir;
        private readonly string productId;
    }
}