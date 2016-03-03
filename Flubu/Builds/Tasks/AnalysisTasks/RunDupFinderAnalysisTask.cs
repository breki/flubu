using System.Globalization;
using System.IO;
using Flubu.Builds.Tasks.NuGetTasks;
using Flubu.Tasks.Processes;
using Flubu.Tasks.Text;

namespace Flubu.Builds.Tasks.AnalysisTasks
{
    public class RunDupFinderAnalysisTask : TaskBase
    {
        public override string Description
        {
            get { return "Run dupFinder analysis on the source code"; }
        }

        public int MaxAllowedDuplicateCost
        {
            get { return maxAllowedDuplicateCost; }
            set { maxAllowedDuplicateCost = value; }
        }

        public double MaxAllowedFragmentsCostRatio
        {
            get { return maxAllowedFragmentsCostRatio; }
            set { maxAllowedFragmentsCostRatio = value; }
        }

        public string Exclude
        {
            get { return exclude; }
            set { exclude = value; }
        }

        public string ExcludeByComment
        {
            get { return excludeByComment; }
            set { excludeByComment = value; }
        }

        public string ExcludeCodeRegions
        {
            get { return excludeCodeRegions; }
            set { excludeCodeRegions = value; }
        }

        protected override void DoExecute (ITaskContext context)
        {
            string dupFinderXmlReportFileName = RunDupFinder (context);

            if (dupFinderXmlReportFileName == null)
                return;

            GenerateDupFinderHtmlReport (context, dupFinderXmlReportFileName);

            AnalyzeDupFinderReport (context, dupFinderXmlReportFileName);
        }

        private string RunDupFinder (ITaskContext context)
        {
            DownloadNugetPackageInUserRepositoryTask downloadPackageTask = new DownloadNugetPackageInUserRepositoryTask (
                ResharperCmdLineToolsPackageId);
            downloadPackageTask.Execute (context);

            string dupFinderExeFileName = Path.Combine (downloadPackageTask.PackageDirectory, "tools/dupfinder.exe");

            if (!File.Exists (dupFinderExeFileName))
            {
                context.WriteMessage (
                    TaskMessageLevel.Warn,
                    "R# dupfinder is not present in the expected location ('{0}'), cannot run source code duplicates analysis",
                    dupFinderExeFileName);
                return null;
            }

            string buildsDir = context.Properties[BuildProps.BuildDir];
            string dupFinderXmlReportFileName = Path.Combine (buildsDir, "dupfinder-report.xml");

            IRunProgramTask task = new RunProgramTask (
                dupFinderExeFileName)
                .AddArgument ("--output={0}", dupFinderXmlReportFileName)
                .AddArgument ("--show-text");

            if (exclude != null)
                task.AddArgument ("--exclude={0}", exclude);
            if (excludeByComment != null)
                task.AddArgument ("--exclude-by-comment={0}", excludeByComment);
            if (excludeCodeRegions != null)
                task.AddArgument ("--exclude-code-regions={0}", excludeCodeRegions);

            task
                .AddArgument (context.Properties[BuildProps.SolutionFileName])
                .Execute (context);

            return dupFinderXmlReportFileName;
        }

        private static void GenerateDupFinderHtmlReport (ITaskContext context, string dupFinderXmlReportFileName)
        {
            string buildsDir = context.Properties[BuildProps.BuildDir];

            const string DupFinderXsltReportFileName = @"lib\ReSharper.CommandLineTools\dupfinder.xsl";
            string dupFinderHtmlReportFileName = Path.Combine (buildsDir, "dupfinder-report.html");

            XsltTransformTask reportToHtmlTask = new XsltTransformTask (
                dupFinderXmlReportFileName,
                dupFinderHtmlReportFileName,
                DupFinderXsltReportFileName);
            reportToHtmlTask.Execute (context);

            context.WriteInfo (@"Duplicates HTML report was written to {0}", Path.GetFullPath (dupFinderHtmlReportFileName));
        }

        private void AnalyzeDupFinderReport (ITaskContext context, string dupFinderXmlReportFileName)
        {
            const string PropertyCodebaseCost = "CodebaseCost";
            const string PropertyTotalDuplicatesCost = "TotalDuplicatesCost";
            const string PropertyTotalFragmentsCost = "TotalFragmentsCost";
            const string PropertyDuplicatesCount = "DuplicatesCount";
            string xPathOverThresholdDuplicatesCount = string.Format (
                CultureInfo.InvariantCulture, "count(/DuplicatesReport/Duplicates/Duplicate[@Cost>{0}])", maxAllowedDuplicateCost);

            EvaluateXmlTask findViolationsTask = new EvaluateXmlTask (dupFinderXmlReportFileName)
                .AddExpression (PropertyCodebaseCost, "sum(/DuplicatesReport/Statistics/CodebaseCost)")
                .AddExpression (PropertyTotalDuplicatesCost, "sum(/DuplicatesReport/Statistics/TotalDuplicatesCost)")
                .AddExpression (PropertyTotalFragmentsCost, "sum(/DuplicatesReport/Statistics/TotalFragmentsCost)")
                .AddExpression (PropertyDuplicatesCount, xPathOverThresholdDuplicatesCount);
            findViolationsTask.Execute (context);

            bool shouldFailBuild = false;

            int? duplicatesCount = GetDupFinderProperyValue (context, PropertyDuplicatesCount);
            if (duplicatesCount.HasValue && duplicatesCount > 0)
            {
                context.WriteMessage (
                    TaskMessageLevel.Warn,
                    "There are {0} code duplicates that are above the {1} cost threshold",
                    duplicatesCount,
                    maxAllowedDuplicateCost);
                shouldFailBuild = true;
            }

            int? codebaseCost = GetDupFinderProperyValue (context, PropertyCodebaseCost);
            int? totalFragmentsCost = GetDupFinderProperyValue (context, PropertyTotalFragmentsCost);
            if (codebaseCost.HasValue && totalFragmentsCost.HasValue && codebaseCost > 0)
            {
                double fragmentsCostRatio = ((double)totalFragmentsCost.Value) / codebaseCost.Value;
                if (fragmentsCostRatio >= maxAllowedFragmentsCostRatio)
                {
                    context.WriteMessage (
                        TaskMessageLevel.Warn,
                        "The fragments cost ratio ({0:P}) is above the max. allowed threshold ({1:P})",
                        fragmentsCostRatio,
                        maxAllowedFragmentsCostRatio);
                    shouldFailBuild = true;
                }
                else
                    context.WriteInfo ("Fragments cost ratio is: {0:P}", fragmentsCostRatio);
            }

            if (shouldFailBuild)
                context.Fail ("Failing the build because of code duplication");
        }

        private static int? GetDupFinderProperyValue (ITaskContext context, string propertyName)
        {
            string valueStr = context.Properties[propertyName];
            if (valueStr == null)
                return null;

            return int.Parse (valueStr, CultureInfo.InvariantCulture);
        }

        private const string ResharperCmdLineToolsPackageId = "JetBrains.ReSharper.CommandLineTools";
        private int maxAllowedDuplicateCost = 300;
        private double maxAllowedFragmentsCostRatio = 0.1;
        private string exclude;
        private string excludeByComment;
        private string excludeCodeRegions;
    }
}