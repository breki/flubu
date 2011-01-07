using System;
using System.Collections.Generic;
using System.IO;
using Flubu.Builds.VSSolutionBrowsing;

namespace Flubu.Builds
{
    /// <summary>
    /// Registry of products of a build. The products will be collected, packaged and sent to the build server's artifact directory
    /// after a successful build.
    /// </summary>
    /// <typeparam name="TRunner">The concrete type of the runner.</typeparam>
    public class BuildProductsRegistry<TRunner>
        where TRunner : BuildRunner<TRunner>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildProductsRegistry{TRunner}"/> class.
        /// </summary>
        /// <param name="runner">The build runner to use.</param>
        public BuildProductsRegistry(TRunner runner)
        {
            this.runner = runner;
        }

        /// <summary>
        /// Gets a value indicating whether this instance of build products registry has any products.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has any products; otherwise, <c>false</c>.
        /// </value>
        public bool HasAnyProducts
        {
            get { return buildProducts.Count > 0; }
        }

        /// <summary>
        /// Adds all VisualStudio Web projects registered in the solution to the list of build products.
        /// </summary>
        /// <param name="productPartId">The ID of the product part Web projects will belong to.</param>
        /// <param name="includeDebugFiles">Indicates whether the PDB files should be included in the package.</param>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> AddAllWebProjects (string productPartId, bool includeDebugFiles)
        {
            foreach (VSProjectExtendedInfo projectExtendedInfo in runner.ProjectExtendedInfos.Values)
            {
                if (projectExtendedInfo.IsWebProject)
                    AddProject (productPartId, projectExtendedInfo.ProjectInfo.ProjectName, includeDebugFiles);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified directory to the list of build products.
        /// </summary>
        /// <param name="productPartId">The ID of the product part this directory belongs to.</param>
        /// <param name="sourceDirectoryPath">The source directory path from where the files will be copied.</param>
        /// <param name="productDirectoryPath">The path relative to the package directory where this product's files will be copied.</param>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> AddDirectory(
            string productPartId, 
            string sourceDirectoryPath,
            string productDirectoryPath)
        {
            buildProducts.Add(
                new SimpleBuildProduct<TRunner>(
                    productPartId, 
                    sourceDirectoryPath, 
                    productDirectoryPath, 
                    null, 
                    null));
            return this;
        }

        /// <summary>
        /// Adds the specified directory to the list of build products.
        /// </summary>
        /// <param name="productPartId">The ID of the product part this directory belongs to.</param>
        /// <param name="sourceDirectoryPath">The source directory path from where the files will be copied.</param>
        /// <param name="productDirectoryPath">The path relative to the package directory where this product's files will be copied.</param>
        /// <param name="inclusionPattern">The inclusion regular expression pattern for files.</param>
        /// <param name="exclusionPattern">The exclusion regular expression pattern for files.</param>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> AddDirectory(
            string productPartId,
            string sourceDirectoryPath,
            string productDirectoryPath,
            string inclusionPattern,
            string exclusionPattern)
        {
            buildProducts.Add(
                new SimpleBuildProduct<TRunner>(
                    productPartId, 
                    sourceDirectoryPath, 
                    productDirectoryPath, 
                    inclusionPattern, 
                    exclusionPattern));
            return this;
        }

        /// <summary>
        /// Adds the specified file to the list of build products.
        /// </summary>
        /// <param name="productPartId">The ID of the product part this directory belongs to.</param>
        /// <param name="sourceFileName">Name of the file to be added.</param>
        /// <param name="destinationFileName">The path relative to the package directory where this file will be copied.</param>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> AddFile (string productPartId, string sourceFileName, string destinationFileName)
        {
            buildProducts.Add(
                new FileBuildProduct<TRunner>(productPartId, sourceFileName, destinationFileName));
            return this;
        }

        /// <summary>
        /// Adds the specified VisualStudio project to the list of build products.
        /// </summary>
        /// <param name="productPartId">The ID of the product part this project belongs to.</param>
        /// <param name="projectName">Name of the project. This name will be used as a directory name where all the product's
        /// files will be copied.</param>
        /// <param name="includeDebugFiles">Indicates whether the PDB files should be included in the package.</param>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> AddProject(
            string productPartId, 
            string projectName,
            bool includeDebugFiles)
        {
            string productDirectoryName = projectName;
            return this.AddProject(
                productPartId, 
                projectName, 
                productDirectoryName,
                includeDebugFiles);
        }

        /// <summary>
        /// Adds the specified VisualStudio project to the list of build products.
        /// </summary>
        /// <param name="productPartId">The ID of the product part this project belongs to.</param>
        /// <param name="projectName">Name of the project.</param>
        /// <param name="productDirectoryPath">The path relative to the package directory where this product's files will be copied.
        /// If set to <see cref="string.Empty"/>, the files will be copied directly on the top-level directory.</param>
        /// <param name="includeDebugFiles">Indicates whether the PDB files should be included in the package.</param>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> AddProject (
            string productPartId, 
            string projectName, 
            string productDirectoryPath,
            bool includeDebugFiles)
        {
            VSProjectWithFileInfo projectWithFileInfo = (VSProjectWithFileInfo) runner.Solution.FindProjectByName (projectName);

            if (runner.ProjectExtendedInfos.ContainsKey (projectName))
            {
                VSProjectExtendedInfo extendedInfo = runner.ProjectExtendedInfos[projectName];
                if (extendedInfo.IsWebProject)
                {
                    buildProducts.Add (
                        new WebApplicationBuildProduct<TRunner> (
                            productPartId, 
                            projectWithFileInfo.ProjectDirectoryPath, 
                            productDirectoryPath));
                    return this;
                }
            }

            string excludeFilter = null;
            if (false == includeDebugFiles)
                excludeFilter = @".*\.pdb$";

            buildProducts.Add (
                new SimpleBuildProduct<TRunner> (
                    productPartId,
                    Path.Combine (projectWithFileInfo.ProjectDirectoryPath, runner.GetProjectOutputPath (projectName)),
                    productDirectoryPath,
                    null,
                    excludeFilter));

            return this;            
        }

        /// <summary>
        /// Clears the build products registry allowing a new products set to be organized.
        /// </summary>
        /// <returns>
        /// This same instance of the <see cref="BuildProductsRegistry{TRunner}"/>.
        /// </returns>
        public BuildProductsRegistry<TRunner> ClearRegistry()
        {
            buildProducts.Clear();
            return this;
        }

        public void CopyProducts(string packageDirectory)
        {
            foreach (BuildProduct<TRunner> product in buildProducts)
                product.CopyProductFiles(runner, packageDirectory);
        }

        /// <summary>
        /// Lists all of files which belong to the specified product parts.
        /// </summary>
        /// <param name="ids">The IDs of product parts. If the list is empty, the method will use all available product parts.</param>
        /// <returns>A list of files files which belong to the specified product parts.</returns>
        public IEnumerable<string> ListFilesForProductParts(string[] ids)
        {
            List<string> files = new List<string>();

            foreach (BuildProduct<TRunner> buildProduct in buildProducts)
            {
                int index = 0;

                if (ids.Length > 0)
                {
                    index = Array.FindIndex(
                        ids,
                        match => 0 == String.Compare(
                                          match,
                                          buildProduct.ProductPartId,
                                          StringComparison.OrdinalIgnoreCase));
                }

                // if this product is in the list of product parts, use its files
                if (index != -1)
                    files.AddRange(buildProduct.ListCopiedFiles());
            }

            return files;
        }

        private readonly TRunner runner;
        private List<BuildProduct<TRunner>> buildProducts = new List<BuildProduct<TRunner>>();
    }
}