using System;
using System.Globalization;
using System.IO;

namespace Flubu.Builds.Tasks
{
    public class FetchBuildVersionFromHudsonTask : TaskBase, IFetchBuildVersionTask
    {
        public FetchBuildVersionFromHudsonTask(
            string productRootDir,
            string productId)
        {
            this.productRootDir = productRootDir;
            this.productId = productId;
        }

        public override string Description
        {
            get { return "Fetch the build version from Hudson"; }
        }

        public Version BuildVersion
        {
            get { return buildVersion; }
        }

        protected override void DoExecute(ITaskContext context)
        {
            if (!HudsonHelper.IsRunningUnderHudson)
                throw new InvalidOperationException("This task can only be called when the script is run under Hudson");

            string projectVersionFileName =
                Path.Combine(productRootDir, productId + ".ProjectVersion.txt");

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

            string hudsonBuildNumberString = Environment.GetEnvironmentVariable("BUILD_NUMBER");
            int hudsonBuildNumber = int.Parse(hudsonBuildNumberString, CultureInfo.InvariantCulture);

            string svnRevisionNumberString = Environment.GetEnvironmentVariable("SVN_REVISION");
            int svnRevisionNumber = int.Parse(svnRevisionNumberString, CultureInfo.InvariantCulture);

            buildVersion = new Version(
                BuildVersion.Major,
                BuildVersion.Minor,
                svnRevisionNumber,
                hudsonBuildNumber);

            context.WriteInfo("Project build version: {0}", buildVersion);
        }

        private Version buildVersion;
        private readonly string productRootDir;
        private readonly string productId;
    }
}