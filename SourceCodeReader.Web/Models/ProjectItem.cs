using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public enum ProjectItemType
    {
        File = 0,
        Directory,
        Invalid
    }

    public class ProjectItem
    {
        public ProjectItem()
        {
            this.Items = new List<ProjectItem>();
        }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Content { get; set; }

        public List<ProjectItem> Items { get; set; }

        public ProjectItemType Type { get; set; }

        public string DownloadedDate { get; set; }
    }
}