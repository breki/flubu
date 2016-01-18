using Flubu.Tasks.Iis.Iis7;

namespace Flubu.Tasks.Iis
{
    public interface ICreateWebSiteTask : ITask
    {
        /// <summary>
        /// set the web site name.
        /// </summary>
        /// <param name="siteName">The web site name.</param>
        /// <returns>The <see cref="Iis7CreateWebSiteTask.CreateWebSiteBindingProtocol"/> instance.</returns>
        Iis7CreateWebSiteTask.CreateWebSiteBindingProtocol WebSiteName(string siteName);

        /// <summary>
        /// Set WebSite mode.
        /// </summary>
        /// <param name="webSiteMode">The website Mode <see cref="CreateWebApplicationMode"/> </param>
        /// <returns>The Iis7CreateWebSiteTask.</returns>
        Iis7CreateWebSiteTask WebSiteMode(CreateWebApplicationMode webSiteMode);

        /// <summary>
        /// Set web site application pool name.
        /// </summary>
        /// <param name="applicationPool">The application pool name</param>
        /// <returns>The  Iis7CreateWebSiteTask.</returns>
        Iis7CreateWebSiteTask ApplicationPoolName(string applicationPool);

        /// <summary>
        ///  Add MimeType. Can be used multiple times.
        /// </summary>
        /// <param name="mimeType">The mime type</param>
        /// <returns>The  Iis7CreateWebSiteTask.</returns>
        Iis7CreateWebSiteTask AddMimeType(MimeType mimeType);
    }
}
