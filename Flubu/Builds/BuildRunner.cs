using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Flubu.Builds.VSSolutionBrowsing;

namespace Flubu.Builds
{
    /// <summary>
    /// A specialization of the <see cref="FlubuRunner{TRunner}"/> oriented towards building .NET solutions.
    /// </summary>
    /// <typeparam name="TRunner">The concrete type of the runner.</typeparam>
    public class BuildRunner<TRunner> : FlubuRunner<TRunner>
        where TRunner : BuildRunner<TRunner>
    {
        public BuildRunner(string productId) : base (productId, @"logs\Flubu.Build.log", 0)
        {
            this.productId = productId;
            productName = productId;
            buildProducts = new BuildProductsRegistry<TRunner>(ReturnThisTRunner());

            // add CCNet listener
            string ccnetListenerFilePath = Environment.GetEnvironmentVariable ("CCNetListenerFile");

            if (false == String.IsNullOrEmpty(ccnetListenerFilePath))
                TaskContext.AddLogger(new FlubuCCNetListener(
                    ccnetListenerFilePath));
        }

        public string BuildConfiguration
        {
            get { return buildConfiguration; }
            set { buildConfiguration = value; }
        }

        public string BuildDir
        {
            get { return buildDir; }
        }

        /// <summary>
        /// Gets the build packages directory.
        /// </summary>
        /// <value>The build packages directory.</value>
        public string BuildPackagesDir
        {
            get { return Path.Combine(BuildDir, "packages"); }
        }

        public BuildProductsRegistry<TRunner> BuildProducts
        {
            get { return buildProducts; }
        }

        public Version BuildVersion
        {
            get { return buildVersion; }
            set { buildVersion = value; }
        }

        public string CcnetArtifactDirectory
        {
            get
            {
                string dir = Environment.GetEnvironmentVariable("CCNetArtifactDirectory");
                if (dir == null)
                    throw new InvalidOperationException("CCNetArtifactDirectory environment variable is missing.");
                return dir;
            }
        }

        public bool IsRunningUnderCruiseControl
        {
            get
            {
                string ccnetBuildCondition = Environment.GetEnvironmentVariable("CCNetBuildCondition");
                return ccnetBuildCondition != null;
            }
        }

        public bool IsRunningUnderHudson
        {
            get
            {
                string hudsonEnv = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                return hudsonEnv != null;
            }
        }

        /// <summary>
        /// Gets the file name of the last zip package that was produced by calling the <see cref="PackageBuildProduct"/> method.
        /// </summary>
        /// <value>The file name of the last zip package.</value>
        public string LastZipPackageFileName
        {
            get { return lastZipPackageFileName; }
        }

        public string LibDir
        {
            get { return libDir; }
        }

        public string ProductId
        {
            get { return productId; }
        }

        public string ProductName
        {
            get { return productName; }
        }

        public string ProductRootDir
        {
            get { return productRootDir; }
        }

        /// <summary>
        /// Gets the dictionary of extended information about VisualStudio projects in the currently
        /// loaded solution. The dictionary key represents the project name.
        /// </summary>
        /// <value>The dictionary of VS project extended information.</value>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Infos")]
        public IDictionary<string, VSProjectExtendedInfo> ProjectExtendedInfos
        {
            get { return projectExtendedInfos; }
        }

        /// <summary>
        /// Gets the <see cref="VSSolution"/> object for the loaded VisualStudio solution.
        /// </summary>
        /// <value>The <see cref="VSSolution"/> object.</value>
        public VSSolution Solution
        {
            get { return solution; }
        }

        public int TestRuns
        {
            get { return testRuns; }
        }

        /// <summary>
        /// Copies all of the build results to the CCNet artifact directory.
        /// </summary>
        /// <returns>The same instance of this <see cref="TRunner"/>.</returns>
        public TRunner CopyBuildLogsToCCNet()
        {
            if (false == IsRunningUnderCruiseControl)
                return ReturnThisTRunner();

            string ccnetArtifactDirectory = CcnetArtifactDirectory;
            Log("Copying build logs to CCNet artifact directory '{0}'", ccnetArtifactDirectory);

            string buildLogsDir = EnsureBuildLogsTestDirectoryExists();

            CopyDirectoryStructure(buildLogsDir, ccnetArtifactDirectory, true);

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Copies the last packaged build product to CruiseControl.NET artifact directory.
        /// </summary>
        /// <param name="buildProductDestinationFileNameFormat">Specifies the format of the build product destination file. The format uses the following
        /// arguments:
        /// <list type="bullet">
        ///         <item>
        ///             <term>{0}</term>
        ///             <description>ProductId</description>
        ///         </item>
        ///         <item>
        ///             <term>{1}</term>
        ///             <description>BuildVersion (4 numbers)</description>
        ///         </item>
        ///         <item>
        ///             <term>{2}</term>
        ///             <description>BuildVersion (major)</description>
        ///         </item>
        ///         <item>
        ///             <term>{3}</term>
        ///             <description>BuildVersion (minor)</description>
        ///         </item>
        ///         <item>
        ///             <term>{4}</term>
        ///             <description>File name of the last zip package (without the directory path)</description>
        ///         </item>
        ///     </list></param>
        /// <returns>The same instance of this <see cref="BuildRunner{TRunner}"/>.</returns>
        public TRunner CopyBuildProductToCCNet(string buildProductDestinationFileNameFormat)
        {
            if (false == IsRunningUnderCruiseControl)
                return ReturnThisTRunner();

            // construct the actual destination path
            string destinationPath = String.Format(
                CultureInfo.InvariantCulture,
                buildProductDestinationFileNameFormat,
                ProductId,
                BuildVersion,
                BuildVersion.Major,
                BuildVersion.Minor,
                Path.GetFileName(LastZipPackageFileName));

            // combine this with the CCNet artifacts directory
            destinationPath = Path.Combine(CcnetArtifactDirectory, destinationPath);

            EnsureDirectoryPathExists(destinationPath, true);
            CopyFile(lastZipPackageFileName, destinationPath, true);

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Executes the specified SQL script file.
        /// </summary>
        /// <param name="scriptFileName">Name of the script file.</param>
        /// <param name="userName">Name of the database user.</param>
        /// <param name="password">The database user's password.</param>
        /// <returns>The same instance of this <see cref="TRunner"/>.</returns>
        public TRunner ExecuteSqlScriptFile(
            string scriptFileName, 
            string userName,
            string password)
        {
            return ExecuteSqlScriptFile(scriptFileName, userName, password, null);
        }

        /// <summary>
        /// Executes the specified SQL script file.
        /// </summary>
        /// <param name="scriptFileName">Name of the script file.</param>
        /// <param name="userName">Name of the database user.</param>
        /// <param name="password">The database user's password.</param>
        /// <param name="useDatabase">The database to run the script under. If <c>null</c>, the script will run under
        /// the default database.</param>
        /// <returns>The same instance of this <see cref="BuildRunner"/>.</returns>
        public TRunner ExecuteSqlScriptFile(
            string scriptFileName, 
            string userName,
            string password,
            string useDatabase)
        {
            ProgramRunner
                .AddArgument("-U")
                .AddArgument(userName)
                .AddArgument("-P")
                .AddArgument(password)
                .AddArgument("-i")
                .AddArgument(scriptFileName)
                .AddArgument("-r1")
                .AddArgument("-m-1");

            if (useDatabase != null)
            {
                ProgramRunner
                    .AddArgument("-d")
                    .AddArgument(useDatabase);
            }

            ProgramRunner.Run(@"C:\Program Files\Microsoft SQL Server\90\Tools\Binn\osql.exe", false);

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Fetches the build version, either from the local version info file or from CCNet
        /// (if the build is running under CCNet).
        /// </summary>
        /// <returns>
        /// The same instance of this <see cref="TRunner"/>.
        /// </returns>
        public TRunner FetchBuildVersion()
        {
            return FetchBuildVersion(true);
        }

        /// <summary>
        /// Fetches the build version, either from the local version info file or from CCNet
        /// (if the build is running under CCNet).
        /// </summary>
        /// <param name="loadFromFile">If not running under CCNet, fetch version from file or not.</param>
        /// <returns>
        /// The same instance of this <see cref="TRunner"/>.
        /// </returns>
        public TRunner FetchBuildVersion(bool loadFromFile)
        {
            TaskContext.LogTaskStarted("Fetching the build version");

            if (IsRunningUnderCruiseControl)
            {
                string ccnetLabel = Environment.GetEnvironmentVariable("CCNetLabel");
                Regex ccnetLabelRegex = new Regex(@"(?<version>([0-9]+.){3}[0-9]+)", RegexOptions.ExplicitCapture);
                Match match = ccnetLabelRegex.Match(ccnetLabel);
                if (false == match.Success)
                {
                    Log(
                        "CCNet build label '{0}' could not be parsed to extract the build version. Using the default version instead",
                        ccnetLabel);
                }
                else
                    BuildVersion = new Version(match.Groups["version"].Value);
            }
            else if (loadFromFile)
            {
                string projectVersionFileName = MakePathFromRootDir(ProductId) + ".ProjectVersion.txt";

                if (false == File.Exists(projectVersionFileName))
                    Fail("Project version file ('{0}') is missing.", projectVersionFileName);

                using (Stream stream = File.Open(projectVersionFileName, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string versionAsString = reader.ReadLine();
                        BuildVersion = new Version(versionAsString);
                    }
                }
            }

            Log("Project build version: {0}", BuildVersion);

            TaskContext.LogTaskFinished();
            return ReturnThisTRunner();
        }

        public TRunner FxCop()
        {
            TaskContext.LogTaskStarted("Running FxCop analysis");

            string fxProjectPath = MakePathFromRootDir(productId) + ".FxCop";

            AssertFileExists("FxCop project file", fxProjectPath);

            string fxReportPath = EnsureBuildLogsTestDirectoryExists();
            fxReportPath = Path.Combine(fxReportPath, productId);
            fxReportPath = String.Format(
                CultureInfo.InvariantCulture, 
                "{0}.FxCopReport.xml", 
                fxReportPath);

            ProgramRunner
                .AddArgument(@"/project:{0}", fxProjectPath)
                .AddArgument(@"/out:{0}", fxReportPath)
                .AddArgument(@"/dictionary:CustomDictionary.xml")
                .AddArgument(@"/ignoregeneratedcode");
            
            string fxCopCmdPath = MakePathFromRootDir(
                Path.Combine(libDir, @"Microsoft FxCop 1.36\FxCopCmd.exe"));
            AssertFileExists("FxCopCmd.exe", fxCopCmdPath);
            ProgramRunner.Run(fxCopCmdPath, true);

            // check if the report file was generated
            bool isReportFileGenerated = File.Exists(fxReportPath);

            // FxCop returns different exit codes for different things
            // see http://msdn.microsoft.com/en-us/library/bb164705(VS.80).aspx for the list of exit codes
            // exit code 4 means "Project load error" but it occurs when the old FxCop violations exist
            // which are then removed from the code
            if ((ProgramRunner.LastExitCode != 0 && ProgramRunner.LastExitCode != 4) || isReportFileGenerated)
            {
                if (IsRunningUnderCruiseControl)
                {
                    if (File.Exists(fxReportPath))
                    {
                        string fxcopReportFileName = Path.GetFileName(fxReportPath);
                        try
                        {
                            CopyFile(fxReportPath, Path.Combine(ccnetDir, fxcopReportFileName), true);
                        }
                        catch (IOException)
                        {
                            Log(
                                "Warning: could not copy FxCop report file '{0}' to the CC.NET dir",
                                fxReportPath);
                        }                        
                    }
                }
                else if (IsRunningUnderHudson)
                {
                }
                else
                {
                    // run FxCop GUI
                    ProgramRunner
                        .AddArgument(fxProjectPath)
                        .Run(MakePathFromRootDir(Path.Combine(libDir, @"Microsoft FxCop 1.36\FxCop.exe")));
                }

                Fail("FxCop found violations in the code.");
            }

            TaskContext.LogTaskFinished();
            return ReturnThisTRunner();
        }

        public TRunner Gendarme(params string[] projectNames)
        {
            string reportDir = EnsureBuildLogsTestDirectoryExists();
            string reportFileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.GendarmeReport.xml", 
                ProductId);
            string gendarmeXmlReportFile = Path.Combine(reportDir, reportFileName);

            ProgramRunner
                .AddArgument("--html")
                .AddArgument("{0}.GendarmeReport.html", ProductId)
                .AddArgument("--xml")
                .AddArgument(gendarmeXmlReportFile)
                .AddArgument("--severity")
                .AddArgument("all");

            new List<string>(projectNames).ForEach(
                projectName =>
                {
                    string outputPath = GetProjectOutputPath(projectName);
                    if (outputPath == null)
                        return;

                    VSProjectWithFileInfo projectInfo = (VSProjectWithFileInfo)Solution.FindProjectByName(projectName);

                    string projectOutputType = projectInfo.Project.Properties["OutputType"];
                    string projectAssemblyName = projectInfo.Project.Properties["AssemblyName"];
                    string projectAssemblyFileName = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}.{1}",
                        projectAssemblyName,
                        projectOutputType == "Library" ? "dll" : "exe");

                    string projectAssemblyFullPath = Path.Combine(
                        Path.Combine(
                            projectInfo.ProjectDirectoryPath,
                            outputPath),
                        projectAssemblyFileName);

                    ProgramRunner.AddArgument(projectAssemblyFullPath);
                });

            string gendarmePath = MakePathFromRootDir(LibDir + @"\Gendarme\gendarme.exe");
            AssertFileExists("Gendarme.exe", gendarmePath);
            ProgramRunner.Run(gendarmePath, true);

            return ReturnThisTRunner();
        }

        public TRunner GenerateCommonAssemblyInfo()
        {
            return GenerateCommonAssemblyInfo(true, true, false);
        }

        /// <summary>
        /// Gets the output path for a specified VisualStudio project. The output path is relative
        /// to the directory where the project file is located.
        /// </summary>
        /// <param name="projectName">Project name.</param>
        /// <returns>The output path or <c>null</c> if the project is not compatibile.</returns>
        /// <exception cref="ArgumentException">The method could not extract the data from the project file.</exception>
        public string GetProjectOutputPath(string projectName)
        {
            VSProjectWithFileInfo projectWithFileInfo = (VSProjectWithFileInfo)solution.FindProjectByName(projectName);
            return GetProjectOutputPath(projectWithFileInfo);
        }

        public TRunner HudsonFetchBuildVersion()
        {
            TaskContext.LogTaskStarted("Fetching the build version");

            if (IsRunningUnderHudson)
            {
                TaskContext.LogMessage("Running under Hudson");

                string projectVersionFileName = MakePathFromRootDir(ProductId) + ".ProjectVersion.txt";

                if (false == File.Exists(projectVersionFileName))
                    Fail("Project version file ('{0}') is missing.", projectVersionFileName);

                using (Stream stream = File.Open(projectVersionFileName, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string versionAsString = reader.ReadLine();
                        BuildVersion = new Version(versionAsString);
                    }
                }

                string hudsonBuildNumberString = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                int hudsonBuildNumber = int.Parse(hudsonBuildNumberString, CultureInfo.InvariantCulture);

                string svnRevisionNumberString = Environment.GetEnvironmentVariable("SVN_REVISION");
                int svnRevisionNumber = int.Parse(svnRevisionNumberString, CultureInfo.InvariantCulture);

                BuildVersion = new Version(
                    BuildVersion.Major,
                    BuildVersion.Minor,
                    svnRevisionNumber,
                    hudsonBuildNumber);

                Log("Project build version: {0}", BuildVersion);

                TaskContext.LogTaskFinished();
                return ReturnThisTRunner();
            }

            return FetchBuildVersion();
        }

        /// <summary>
        /// Loads the specified VisualStudio solution and all of its projects.
        /// </summary>
        /// <param name="solutionFileName">Name of the solution file.</param>
        /// <returns>The same instance of this <see cref="TRunner"/>.</returns>
        public TRunner LoadSolution(string solutionFileName)
        {
            solutionFileName = MakePathFromRootDir(solutionFileName);
            AssertFileExists("VisualStudio solution file", solutionFileName);
            solution = VSSolution.Load(solutionFileName);

            solution.ForEachProject(delegate (VSProjectInfo projectInfo)
                                        {
                                            if (projectInfo.ProjectTypeGuid != VSProjectType.CSharpProjectType.ProjectTypeGuid)
                                                return;

                                            projectExtendedInfos.Add(
                                                projectInfo.ProjectName, 
                                                new VSProjectExtendedInfo(projectInfo));
                                        });

            // also load project files
            solution.LoadProjects();

            return ReturnThisTRunner();
        }

        public TRunner MergeCoverageReports()
        {
            // do this only if there were actual tests run
            if (coverageResultsExist)
            {
                string buildLogsDir = EnsureBuildLogsTestDirectoryExists();
                string ncoverExplorerConfigFileName = MakePathFromRootDir(ProductId) + ".NCoverExplorer.config";

                ProgramRunner
                    .AddArgument(Path.Combine(buildLogsDir, "Coverage*.xml"));
                if (File.Exists(ncoverExplorerConfigFileName))
                    ProgramRunner
                        .AddArgument("/c:{0}", ncoverExplorerConfigFileName);
                ProgramRunner
                    .AddArgument("/x:{0}", Path.Combine(buildLogsDir, "MergedCoverageReport.xml"))
                    .AddArgument("/r:5")
                    .AddArgument("/p:{0}", ProductName)
                    .Run(Path.Combine(libDir, @"NCoverExplorer-1.3.6.36\NCoverExplorer.Console.exe"));
            }

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Packages the build product into a zip file.
        /// </summary>
        /// <param name="zipFileNameFormat">
        /// The zip file name format. The format uses the following arguments:
        /// <list type="bullet">
        ///     <item>
        ///         <term>{0}</term>
        ///         <description>ProductId</description>
        ///     </item>
        ///     <item>
        ///         <term>{1}</term>
        ///         <description>BuildVersion (4 numbers)</description>
        ///     </item>
        /// </list> 
        /// </param>
        /// <param name="rootDirectoryForProductFormat">The format for the root directory of the product. This will
        /// be the root directory inside the package ZIP file. 
        /// The format is the same as for the <see cref="zipFileNameFormat"/> parameter.</param>
        /// <param name="productPartsIds">The list of product parts IDs which should be included in the package.
        /// If the list is empty, the method packages all available product part IDs.</param>
        /// <returns>The same instance of this <see cref="TRunner"/>.</returns>
        public TRunner PackageBuildProduct(
            string zipFileNameFormat,
            string rootDirectoryForProductFormat,
            params string[] productPartsIds)
        {
            CopyBuildProductFiles(rootDirectoryForProductFormat, productPartsIds);

            IEnumerable<string> filesToZip = buildProducts.ListFilesForProductParts(productPartsIds);

            // remove duplicate files to avoid problems with the ZIP file
            Dictionary<string, string> filesNoDuplicates = new Dictionary<string, string>();

            foreach (string fileName in filesToZip)
            {
                if (false == filesNoDuplicates.ContainsKey(fileName))
                    filesNoDuplicates.Add(fileName, null);
            }

            string zipFileName = String.Format(
                CultureInfo.InvariantCulture,
                zipFileNameFormat,
                ProductId,
                BuildVersion);

            string packagesDir = MakePathFromRootDir(BuildPackagesDir);
            zipFileName = Path.Combine(packagesDir, zipFileName);
            
            ZipFilesTask task = new ZipFilesTask(
                zipFileName, 
                Path.GetFullPath(MakePathFromRootDir(BuildPackagesDir)), 
                filesNoDuplicates.Keys);
            RunTask(task);

            lastZipPackageFileName = zipFileName;

            return ReturnThisTRunner();
        }

        public TRunner PrepareBuildDirectory()
        {
            TaskContext.LogTaskStarted("Preparing the build directory");

            string fullBuildDir = MakePathFromRootDir(buildDir);

            DeleteDirectory(fullBuildDir, false);
            CreateDirectory(fullBuildDir, true);

            TaskContext.LogTaskFinished();

            return ReturnThisTRunner();
        }

        public TRunner PrepareDatabase(
            string dbScriptsDirectory,
            string dbName,
            string dbUserName,
            string dbUserPassword)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties.Add("ScriptDBName", dbName);
            properties.Add("ScriptDBUserName", dbUserName);
            properties.Add("ScriptDBUserPassword", dbUserPassword);

            FormatString("{0}.*.sql", dbName);
            string expandedFilesDirectory = MakePathFromRootDir(Path.Combine(dbScriptsDirectory, "expanded"));

            // create the "expanded" subdirectory if it doesn't already exist
            CreateDirectory(expandedFilesDirectory, false);

            // expand all .SQL files for this database and store them into a separate directory
            ForEachFile(
                dbScriptsDirectory, 
                "*.sql", 
                file => ExpandProperties(
                  file, 
                  Path.Combine(expandedFilesDirectory, Path.GetFileName(file)),
                  Encoding.UTF8,
                  new UTF8Encoding(false), 
                  properties));

            string dbAdminUserName = "BuildMaster";
            string dbAdminPassword = "masterbuild";

            string sqlFileToExecute = FormatString("{0}.CreateDatabase.sql", dbName);
            sqlFileToExecute = Path.Combine(expandedFilesDirectory, sqlFileToExecute);
            ExecuteSqlScriptFile(sqlFileToExecute, dbAdminUserName, dbAdminPassword);

            sqlFileToExecute = FormatString("{0}.DbScript.sql", dbName);
            sqlFileToExecute = Path.Combine(expandedFilesDirectory, sqlFileToExecute);
            ExecuteSqlScriptFile(sqlFileToExecute, dbAdminUserName, dbAdminPassword, dbName);

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Registers the specified project as a web project.
        /// </summary>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="webApplicationUrl">The project's Web application URL.</param>
        /// <returns>The same instance of this <see cref="BuildRunner{TRunner}"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#")]
        public TRunner RegisterAsWebProject(string projectName, string webApplicationUrl)
        {
            return RegisterAsWebProject(projectName, webApplicationUrl, null);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#")]
        public TRunner RegisterAsWebProject(
            string projectName, 
            string webApplicationUrl,
            string applicationPoolName)
        {
            VSProjectExtendedInfo projectExtendedInfo = projectExtendedInfos[projectName];
            projectExtendedInfo.IsWebProject = true;
            projectExtendedInfo.WebApplicationUrl = new Uri(webApplicationUrl);
            projectExtendedInfo.ApplicationPoolName = applicationPoolName;
            return ReturnThisTRunner();
        }

        public TRunner RunTests(string projectName, bool collectCoverageData)
        {
            Log("Runs tests on '{0}' assembly ({1})", projectName, collectCoverageData ? "with coverage" : "without coverage");

            string buildLogsDir = EnsureBuildLogsTestDirectoryExists();

            VSProjectWithFileInfo testProjectWithFileInfo = (VSProjectWithFileInfo) Solution.FindProjectByName(projectName);
            string projectTargetPath = GetProjectOutputPath (testProjectWithFileInfo.ProjectName);
            projectTargetPath = Path.Combine (testProjectWithFileInfo.ProjectDirectoryPath, projectTargetPath);
            projectTargetPath = projectTargetPath + testProjectWithFileInfo.ProjectName + ".dll";
            projectTargetPath = Path.GetFullPath (MakePathFromRootDir (projectTargetPath));

            string gallioEchoExePath = MakePathFromRootDir(Path.Combine(libDir, @"Gallio\bin\Gallio.Echo.exe"));

            try
            {
                if (collectCoverageData)
                {
                    ProgramRunner
                        .AddArgument(Path.Combine(libDir, @"NCover v1.5.8\CoverLib.dll"))
                        .AddArgument("/s")
                        .Run("regsvr32");

                    ProgramRunner
                        .AddArgument(gallioEchoExePath);
                }

                ProgramRunner
                    .AddArgument(projectTargetPath)
                    .AddArgument("/report-directory:{0}", buildLogsDir)
                    .AddArgument("/report-name-format:TestResults-{0}", TestRuns)
                    .AddArgument("/report-type:xml")
                    .AddArgument("/verbosity:verbose");

                if (collectCoverageData)
                {
                    ProgramRunner
                        .AddArgument("//x")
                        .AddArgument(@"{0}\Coverage-{1}.xml", buildLogsDir, TestRuns)
                        .AddArgument("//ea")
                        .AddArgument("MbUnit.Framework.TestFixtureAttribute")
                        .AddArgument("//w")
                        .AddArgument(Path.GetDirectoryName(projectTargetPath))
                        .AddArgument("//v")
                        .Run(MakePathFromRootDir(Path.Combine(libDir, @"NCover v1.5.8\NCover.Console.exe")));

                    coverageResultsExist = true;
                }
                else
                    ProgramRunner.Run(gallioEchoExePath);

                IncrementTestRunsCounter();
            }
            finally
            {
                if (collectCoverageData)
                {
                    ProgramRunner
                        .AddArgument(Path.Combine(libDir, @"NCover v1.5.8\CoverLib.dll"))
                        .AddArgument("/s")
                        .AddArgument("/u")
                        .Run("regsvr32");
                }                
            }

            return ReturnThisTRunner();
        }

        public TRunner SetCompanyInfo (
            string companyName,
            string companyCopyright,
            string companyTrademark)
        {
            this.companyName = companyName;
            this.companyCopyright = companyCopyright;
            this.companyTrademark = companyTrademark;

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Sets the root path to the 3rd party libraries directory.
        /// </summary>
        /// <param name="libDir">The root path to the 3rd party libraries directory..</param>
        /// <returns>The same instance of this <see cref="TRunner"/>.</returns>
        public TRunner SetLibrariesDirectory (string libDir)
        {
            this.libDir = libDir;
            return ReturnThisTRunner();
        }

        public TRunner SetProductName(string productName)
        {
            this.productName = productName;
            return ReturnThisTRunner();
        }

        public TRunner SetProductRootDir (string productRootDir)
        {
            this.productRootDir = productRootDir;
            return ReturnThisTRunner();
        }

        public TRunner SourceMonitor()
        {
            string sourceMonitorProjectFile = MakePathFromRootDir(ProductId + ".smp");
            AssertFileExists("SourceMonitor project file", sourceMonitorProjectFile);

            string copyOfProjectFile = MakePathFromRootDir(ProductId + ".copy.smp");

            CopyFile(sourceMonitorProjectFile, copyOfProjectFile, true);

            string sourceMonitorCommandFile = MakePathFromRootDir("SourceMonitor.Commands.xml");

            string reportDir = EnsureBuildLogsTestDirectoryExists();
            string sourceMonitorCommands = ConstructSourceMonitorCommands(
                copyOfProjectFile,
                reportDir);
            File.WriteAllText(
                sourceMonitorCommandFile,
                sourceMonitorCommands);

            ProgramRunner
                .AddArgument("/C")
                .AddArgument(sourceMonitorCommandFile)
                .Run(MakePathFromRootDir(LibDir + @"\SourceMonitor\SourceMonitor.exe"));

            return ReturnThisTRunner();
        }

        protected static string ConstructSourceMonitorCommands(
            string sourceMonitorProjectFile,
            string reportsOutputDir)
        {
            const string SourceMonitorCommandsFormat = @"<?xml version='1.0' encoding='utf-8'?>
<sourcemonitor_commands>
  <write_log>true</write_log>
  <command>
    <project_file>{0}</project_file>
    <include_subdirectories>true</include_subdirectories>
    <export>
      <export_insert>xml-stylesheet type='text/xsl' href='lib\SourceMonitor\Contributions\van_de_Burgt\SourceMonitor.xslt'</export_insert>
      <export_file>{1}\sm_summary.xml</export_file>
      <export_type>1</export_type>
    </export>
  </command>
  <command>
    <project_file>{0}</project_file>
    <export>
      <export_insert>xml-stylesheet type='text/xsl' href='lib\SourceMonitor\Contributions\van_de_Burgt\SourceMonitor.xslt'</export_insert>
      <export_file>{1}\sm_details.xml</export_file>
      <export_type>2</export_type>
    </export>
    <delete_checkpoint />
  </command>
</sourcemonitor_commands>";

            return string.Format(
                CultureInfo.InvariantCulture,
                SourceMonitorCommandsFormat,
                sourceMonitorProjectFile,
                reportsOutputDir);
        }

        /// <summary>
        /// Copies build products' files to an extra location in order to be able to zip them into packages later.
        /// </summary>
        /// <param name="rootDirectoryForProductFormat">The format for the root directory of the product. This will
        /// be the root directory inside the package ZIP file. 
        /// The format is the same as for the <see cref="PackageBuildProduct"/> method parameters.</param>
        /// <param name="productPartsIds">The list of product parts IDs which should be included in the package.
        /// If the list is empty, the method packages all available product part IDs.</param>
        /// <returns>The same instance of this <see cref="TRunner"/>.</returns>
        protected virtual TRunner CopyBuildProductFiles(
            string rootDirectoryForProductFormat,
            params string[] productPartsIds)
        {
            // skip this step if we don't have any products to package
            if (false == buildProducts.HasAnyProducts)
                return ReturnThisTRunner();

            Log("Packaging build products");

            // create root directory for the package
            string packageName = String.Format(
                CultureInfo.InvariantCulture,
                rootDirectoryForProductFormat,
                ProductId,
                BuildVersion);

            string packagesDir = Path.GetFullPath(MakePathFromRootDir(BuildPackagesDir));
            CreateDirectory(packagesDir, true);
            packagesDir = Path.Combine(packagesDir, packageName);
            CreateDirectory(packagesDir, true);

            // copy all the necessary files to the package directory
            buildProducts.CopyProducts(packagesDir);

            return ReturnThisTRunner();
        }

        /// <summary>
        /// Ensures the build logs directory exists and returns a full path to it.
        /// </summary>
        /// <returns>A full path to the build logs directory.</returns>
        protected string EnsureBuildLogsTestDirectoryExists()
        {
            string buildLogsDir = Path.GetFullPath(Path.Combine(BuildDir, "BuildLogs"));
            CreateDirectory(buildLogsDir, false);
            return buildLogsDir;
        }

        /// <summary>
        /// Increments the test runs counter.
        /// </summary>
        protected void IncrementTestRunsCounter()
        {
            testRuns++;
        }

        protected string MakePathFromRootDir(string fileName)
        {
            return Path.Combine(productRootDir, fileName);
        }

        private string buildConfiguration = "Release";
        private string buildDir = "Builds";
        private readonly BuildProductsRegistry<TRunner> buildProducts;
        private Version buildVersion = new Version("1.0.0.0");
        private string ccnetDir = "CruiseControl";
        private string companyCopyright;
        private string companyName;
        private string companyTrademark;
        private bool coverageResultsExist;
        private string lastZipPackageFileName;
        private string libDir = "lib";
        private readonly string productId;
        private string productName;
        private string productRootDir = String.Empty;
        private readonly Dictionary<string, VSProjectExtendedInfo> projectExtendedInfos = new Dictionary<string, VSProjectExtendedInfo>();
        private VSSolution solution;
        private int testRuns;
    }
}