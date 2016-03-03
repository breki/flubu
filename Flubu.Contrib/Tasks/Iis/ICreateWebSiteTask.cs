using Flubu.Tasks.Iis.Iis7;

namespace Flubu.Tasks.Iis
{
    public interface ICreateWebsiteTask : ITask
    {
        /// <summary>
        /// set the web site name.
        /// </summary>
        /// <param name="siteName">The web site name.</param>
        /// <returns>The <see cref="Iis7CreateWebsiteTask.CreateWebsiteBindingProtocol"/> instance.</returns>
        Iis7CreateWebsiteTask.CreateWebsiteBindingProtocol WebsiteName(string siteName);

        /// <summary>
        /// Set Website mode.
        /// </summary>
        /// <param name="value">The website Mode <see cref="CreateWebApplicationMode"/> </param>
        /// <returns>The Iis7CreateWebSiteTask.</returns>
        Iis7CreateWebsiteTask WebsiteMode(CreateWebApplicationMode value);

        /// <summary>
        /// Set web site application pool name.
        /// </summary>
        /// <param name="applicationPool">The application pool name</param>
        /// <returns>The  Iis7CreateWebSiteTask.</returns>
        Iis7CreateWebsiteTask ApplicationPoolName(string applicationPool);

        /// <summary>
        ///  Add MimeType. Can be used multiple times.
        /// </summary>
        /// <param name="mimeType">The mime type</param>
        /// <returns>The  Iis7CreateWebSiteTask.</returns>
        Iis7CreateWebsiteTask AddMimeType(MimeType mimeType);
    }
}
