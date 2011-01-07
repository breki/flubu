using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;

namespace Flubu.Tasks.FileSystem
{
    /// <summary>
    /// Sets a file access rule for a specified file path and identities.
    /// </summary>
    public class SetAccessRuleTask : TaskBase
    {
        /// <summary>
        /// Gets or sets the file path for which the access rule should be set.
        /// </summary>
        /// <value>The file path for which the access rule should be set.</value>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        /// <summary>
        /// Gets the task description.
        /// </summary>
        /// <value>The task description.</value>
        public override string Description
        {
            get 
            {
                StringBuilder identitiesEnumerated = new StringBuilder ();
                string separator = String.Empty;

                foreach (string identity in identities)
                {
                    identitiesEnumerated.Append (identity);
                    identitiesEnumerated.Append (separator);
                    separator = ", ";
                }

                return String.Format (
                    System.Globalization.CultureInfo.InvariantCulture,
                    "Add access rule '{2} - {3}' to path '{0}' for identities: '{1}'",
                    path, 
                    identitiesEnumerated, 
                    fileSystemRights, 
                    accessControlType);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetAccessRuleTask"/> class using a specified
        /// file path, an identity, file system rights and access control type.
        /// </summary>
        /// <remarks>This method overload should be used when a single identity needs to be specified.</remarks>
        /// <param name="path">The file path.</param>
        /// <param name="identity">Identity (example: "Network Service").</param>
        /// <param name="fileSystemRights">File system rights.</param>
        /// <param name="accessControlType">Type of the access control.</param>
        public SetAccessRuleTask (
            string path, 
            string identity, 
            FileSystemRights fileSystemRights, 
            AccessControlType accessControlType) 
            : this (path, fileSystemRights, accessControlType)
        {
            identities.Add (identity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetAccessRuleTask"/> class using a specified
        /// file path, file system rights and access control type.
        /// </summary>
        /// <remarks>This method overload should be used when the multiple identities need to be specified.</remarks>
        /// <param name="path">The file path.</param>
        /// <param name="fileSystemRights">File system rights.</param>
        /// <param name="accessControlType">Type of the access control.</param>
        public SetAccessRuleTask (
            string path, 
            FileSystemRights fileSystemRights,
            AccessControlType accessControlType)
        {
            this.path = path;
            this.fileSystemRights = fileSystemRights;
            this.accessControlType = accessControlType;
        }

        /// <summary>
        /// Adds an identity to a list of identites for which the access rule will be applied.
        /// </summary>
        /// <param name="identity">The identity.</param>
        public void AddIdentity (string identity)
        {
            identities.Add (identity);
        }

        /// <summary>
        /// Sets a file access rule for a specified file path and identities.
        /// </summary>
        /// <param name="environment">The script execution environment.</param>
        /// <param name="path">The file path.</param>
        /// <param name="identity">Identity (example: "Network Service").</param>
        /// <param name="fileSystemRights">File system rights.</param>
        /// <param name="accessControlType">Type of the access control.</param>
        public static void Execute(
            ITaskContext environment,
            string path, 
            string identity, 
            FileSystemRights fileSystemRights,
            AccessControlType accessControlType)
        {
            SetAccessRuleTask task = new SetAccessRuleTask (path, identity, fileSystemRights, accessControlType);
            task.Execute (environment);
        }

        /// <summary>
        /// Internal task execution code.
        /// </summary>
        /// <param name="context">The script execution environment.</param>
        protected override void DoExecute (ITaskContext context)
        {
            FileSystemSecurity security;
            object fileInfo;
            bool isDir;

            if (Directory.Exists (path))
            {
                isDir = true;
                fileInfo = new DirectoryInfo (path);
                security = (fileInfo as DirectoryInfo).GetAccessControl (AccessControlSections.Access);
            }
            else
            {
                isDir = false;
                fileInfo = new FileInfo (path);
                security = (fileInfo as FileInfo).GetAccessControl (AccessControlSections.Access);
            }

            //AuthorizationRuleCollection rules = security.GetAccessRules (true, true, typeof (NTAccount));

            foreach (string identity in identities)
            {
                FileSystemAccessRule accessRule;

                if (isDir)
                    accessRule = new FileSystemAccessRule (
                        identity, 
                        fileSystemRights,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        accessControlType);
                else
                    accessRule = new FileSystemAccessRule (
                        identity, 
                        fileSystemRights,
                        accessControlType);

                security.SetAccessRule (accessRule);
            }

            if (isDir)
                (fileInfo as DirectoryInfo).SetAccessControl ((DirectorySecurity)security);
            else
                (fileInfo as FileInfo).SetAccessControl ((FileSecurity)security);
        }

        private string path;
        private List<string> identities = new List<string>();
        private FileSystemRights fileSystemRights;
        private AccessControlType accessControlType;
    }
}
