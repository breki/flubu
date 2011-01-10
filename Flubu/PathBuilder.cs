using System;
using System.IO;

namespace Flubu
{
    /// <summary>
    /// A utility class for building file paths.
    /// </summary>
    /// <remarks><see cref="PathBuilder"/> provides a fluent interface for building file paths. 
    /// The class is a immutable class.</remarks>
    [Obsolete]
    public class PathBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PathBuilder"/> class
        /// using a specified path.
        /// </summary>
        /// <param name="path">The path to use.</param>
        public PathBuilder(string path)
        {
            this.path = path;
        }

        public string FileName
        {
            get
            {
                return System.IO.Path.GetFileName(path);
            }
        }

        /// <summary>
        /// Gets the file path contained in this object.
        /// </summary>
        /// <value>The file path contained in this object.</value>
        public string Path
        {
            get { return path; }
        }

        /// <summary>
        /// Gets the path that is a parent to the current path in this object.
        /// </summary>
        /// <value>The parent path.</value>
        public PathBuilder ParentPath
        {
            get
            {
                return new PathBuilder(System.IO.Path.GetDirectoryName(path));
            }
        }

        /// <summary>
        /// Adds the specified file path to the existing path and returns a new <see cref="PathBuilder"/>
        /// object with the combined path.
        /// </summary>
        /// <param name="additionalPath">The path to add.</param>
        /// <returns>A new <see cref="PathBuilder"/> object with the combined path.</returns>
        public PathBuilder Add (string additionalPath)
        {
            string newPath = System.IO.Path.Combine(path, additionalPath);
            return new PathBuilder(newPath);
        }

        /// <summary>
        /// Ensures the path contained in this object actually exists.
        /// </summary>
        /// <returns>This same object.</returns>
        public PathBuilder EnsureExists()
        {
            if (Directory.Exists(path))
                return this;

            string parentPath = System.IO.Path.GetDirectoryName(path);

            if (false == String.IsNullOrEmpty(parentPath) && false == Directory.Exists(parentPath))
                this.ParentPath.EnsureExists();

            Directory.CreateDirectory(path);

            return this;
        }

        /// <summary>
        /// Creates a new <see cref="PathBuilder"/> with the specified path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>A new <see cref="PathBuilder"/> object.</returns>
        public static PathBuilder New (string path)
        {
            PathBuilder pathBuilder = new PathBuilder(path);
            return pathBuilder;
        }

        /// <summary>
        /// Returns a file path contained in this object.
        /// </summary>
        /// <returns>A file path.</returns>
        public override string ToString()
        {
            return path;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Flubu.PathBuilder"/>.
        /// </summary>
        /// <param name="path">The file path to use.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator PathBuilder(string path)
        {
            return new PathBuilder(path);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Flubu.PathBuilder"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="pathBuilder">The path builder object.</param>
        /// <returns>A file path contained in this <see cref="PathBuilder"/> object.</returns>
        public static implicit operator string(PathBuilder pathBuilder)
        {
            return pathBuilder.path;
        }

        private string path;
    }
}