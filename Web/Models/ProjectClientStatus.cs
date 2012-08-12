using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public enum ProjectStatus
    {
        InProgress = 0,
        NotFound,
        Completed,
        Error        
    }

    public class ProjectClientStatus
    {
        
        public string Message { get; set; }

        public ProjectStatus Status { get; set; }
    }
}