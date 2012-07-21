using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SourceCodeReader.Web.Models
{
    public class TokenResult
    {
        public string FileName { get; set; }

        public string Path { get; set; }

        public int Position { get; set; }
    }
}