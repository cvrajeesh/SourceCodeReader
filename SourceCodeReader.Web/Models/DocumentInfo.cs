using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public class DocumentInfo
    {
        public string Name { get; set; }

        public string ProjectPath { get; set; }

        public string SolutionPath { get; set; }
    }
}