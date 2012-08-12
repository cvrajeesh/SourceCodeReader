using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.LanguageServices
{
    public interface ISourceCodeQueryService
    {
        TokenResult FindExact(TokenParameter parameter);

        DocumentInfo GetFileDetails(string filePath);
    }
}
