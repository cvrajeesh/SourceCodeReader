namespace SourceCodeReader.Web.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using SourceCodeReader.Web.Models;
    using System.Net.Http;
   
    public interface ISourceCodeProviderService
    {
        ProjectItem GetContent(string username, string project, string path, ISourceCodeOpeningProgress openingProgressListener);        
    }
}
