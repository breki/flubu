using System.Collections.Generic;

namespace Flubu.Tasks.Iis
{
    public interface ICreateWebApplicationTask : ITask
    {
        CreateWebApplicationMode Mode { get; set; }
        string ApplicationName { get; set; }
        string LocalPath { get; set; }

        bool AllowAnonymous { get; set; }
        bool AllowAuthNtlm { get; set; }
        string AnonymousUserName { get; set; }
        string AnonymousUserPass { get; set; }
        string AppFriendlyName { get; set; }
        string ApplicationPoolName { get; set; }
        bool AspEnableParentPaths { get; set; }
        bool AccessScript { get; set; }
        bool AccessExecute { get; set; }
        string DefaultDoc { get; set; }
        bool EnableDefaultDoc { get; set; }
        string ParentVirtualDirectoryName { get; set; }
        string WebsiteName { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        IList<MimeType> MimeTypes { get; set; }
    }
}