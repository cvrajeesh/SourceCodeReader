using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public class DeclaredItemDocument
    {
        public string Name { get; set; }

        public string Identifier { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public int Location { get; set; }

        public string ProjectPath { get; set; }

        public string SolutionPath { get; set; }
    }
}