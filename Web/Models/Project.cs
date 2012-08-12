using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public class Project
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime LastModified { get; set; }

        public Uri DownloadPackageUrl { get; set; }
    }
}