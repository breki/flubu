using System;

namespace Flubu.Builds
{
    /// <summary>
    /// Various build properties stored in session that are needed for building.
    /// </summary>
    public static class BuildProps
    {
        public const string AutoAssemblyVersion = "AutoAssemblyVersion";
        public const string BuildConfiguration = "BuildConfiguration";
        public const string BuildDir = "BuildDir";
        public const string BuildLogsDir = "BuildLogsDir";
        public const string BuildVersion = "BuildVersion";
        public const string CompanyCopyright = "CompanyCopyright";
        public const string CompanyName = "CompanyName";
        public const string CompanyTrademark = "CompanyTrademark";
        public const string CompileMaxCpuCount = "CompileMaxCpuCount";
        public const string FxCopDir = "FxCopDir";
        public const string InformationalVersion = "InformationalVersion";
        public const string LibDir = "LibDir";
        public const string NUnitConsolePath = "NUnitConsolePath";
        public const string MSBuildPath = "MSBuildPath";

        /// <summary>
        /// Version of the MSBuild tools to use for compilation.
        /// </summary>
        /// <remarks>
        /// If not defined, the latest available version will be used.
        /// Use the version numbers as defined in Registry path HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions (2.0, 3.5, 4.0, 12.0 etc.)
        /// </remarks>
        public const string MSBuildToolsVersion = "MSBuildToolsVersion";
        public const string PackagesDir = "PackagesDir";
        public const string ProductId = "ProductId";
        public const string ProductName = "ProductName";
        public const string ProductRootDir = "ProductRootDir";
        public const string ProductVersionFieldCount = "ProductVersionFieldCount";

        /// <summary>
        /// File name of where project version is read from. Property is optional.
        /// If not set ProductId is used for part of project file name.
        /// It can also contain relative path to the file name.
        /// </summary>
        public const string ProjectVersionFileName = "ProjectVersionFileName";

        /// <summary>
        /// Boolean property which, if set to <c>true</c>, disables the "build successful/build failed" speech at the end of the build.
        /// </summary>
        public const string SpeechDisabled = "SpeechDisabled";
        public const string Solution = "Solution";

        /// <summary>
        /// The solution file name. It can also contain relative path to the file name.
        /// </summary>
        public const string SolutionFileName = "SolutionFileName";
        public const string UseSolutionDirAsMSBuildWorkingDir = "UseSolutionDirAsMSBuildWorkingDir";
        public const string SvnRevisionVariableName = "SvnRevisionName";

        public const string VersionControlSystem = "VersionControlSystem";
    }
}