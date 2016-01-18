using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flubu.Tasks.Iis
{
    public class MimeType
    {
        /// <summary>
        /// File extension of the mime type.
        /// </summary>
        public string FileExtension { get; set; }
        
        /// <summary>
        /// The Mime type.
        /// </summary>
        public string Type { get; set; }
    }
}
