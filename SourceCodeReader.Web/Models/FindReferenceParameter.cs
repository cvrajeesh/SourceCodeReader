using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public class FindReferenceParameter
    {
        public string Username { get; set; }

        public string Project { get; set; }

        public string Path { get; set; }

        public int Position { get; set; }

        public string Text { get; set; }

        public string Kind { get; set; }
    }
}