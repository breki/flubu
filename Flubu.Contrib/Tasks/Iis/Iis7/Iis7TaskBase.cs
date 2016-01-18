﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Administration;

namespace Flubu.Tasks.Iis.Iis7
{
    public abstract class Iis7TaskBase : TaskBase
    {
        /// <summary>
        /// Adds mime types.
        /// </summary>
        /// <param name="config">Config of the website or web application</param>
        /// <param name="mimeTypes">List of mime types to be added</param>
        protected void AddMimeTypes(Microsoft.Web.Administration.Configuration config, IList<MimeType> mimeTypes)
        {
            if (mimeTypes != null && mimeTypes.Count > 0)
            {
                var staticContentSection = config.GetSection("system.webServer/staticContent");
                ConfigurationElementCollection staticContentCollection = staticContentSection.GetCollection();
                foreach (var mimeType in mimeTypes)
                {
                    ConfigurationElement mimeMapElement = staticContentCollection.CreateElement("mimeMap");
                    mimeMapElement["fileExtension"] = mimeType.FileExtension;
                    mimeMapElement["mimeType"] = mimeType.Type;
                    staticContentCollection.Add(mimeMapElement);
                }
            }
        }

        /// <summary>
        /// Checks if web site exists in the iis.
        /// </summary>
        /// <param name="serverManager">the server manager.</param>
        /// <param name="siteName">Site name to be checked if it exists.</param>
        /// <returns>True if web site exists in iis.</returns>
        protected static bool WebsiteExists(ServerManager serverManager, string siteName)
        {
            SiteCollection sitecollection = serverManager.Sites;
            return sitecollection.Any(site => site.Name == siteName);
        }
    }
}
