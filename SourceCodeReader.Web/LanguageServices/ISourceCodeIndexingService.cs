using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceCodeReader.Web.LanguageServices
{
    public interface ISourceCodeIndexingService
    {
        void IndexProject(string username, string projectName, string projectDirectory);
    }
}
