namespace Flubu.Builds
{
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
        public const string FxcopDir = "FxcopDir";
        public const string InformationalVersion = "InformationalVersion";
        public const string LibDir = "LibDir";
        public const string NUnitConsolePath = "NUnitConsolePath";
        public const string PackagesDir = "PackagesDir";
        public const string ProductId = "ProductId";
        public const string ProductName = "ProductName";
        public const string ProductRootDir = "ProductRootDir";
        public const string ProductVersionFieldCount = "ProductVersionFieldCount";

        /// <summary>
        /// Boolean property which, if set to <c>true</c>, disables the "build successful/build failed" speech at the end of the build.
        /// </summary>
        public const string SpeechDisabled = "SpeechDisabled";
        public const string Solution = "Solution";
        public const string SolutionFileName = "SolutionFileName";
        public const string UseSolutionDirAsMsBuildWorkingDir = "UseSolutionDirAsMsBuildWorkingDir";
        public const string SvnRevisionVariableName = "SvnRevisionName";
        public const string TargetDotNetVersion = "TargetDotNetVersion";

        /// <summary>
        /// Version of the MSBUILD tools to use for compilation.
        /// </summary>
        public const string ToolsVersion = "ToolsVersion";
        public const string VersionControlSystem = "VersionControlSystem";
    }
}