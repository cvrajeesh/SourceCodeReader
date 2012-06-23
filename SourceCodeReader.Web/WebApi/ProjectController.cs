using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SourceCodeReader.Web.Models;
using System.Threading.Tasks;
using SourceCodeReader.Web.Services;

namespace SourceCodeReader.Web.WebApi
{
    public class ProjectController : ApiController
    {
        private ISourceCodeProviderService sourceCodeProviderService;

        public ProjectController(ISourceCodeProviderService sourceCodeProviderService)
        {
            this.sourceCodeProviderService = sourceCodeProviderService;            
        }

        public Task<ProjectItem> Get(string username, string project, string path)
        {
            return Task.Factory.StartNew<ProjectItem>(() =>
                {
                    return this.sourceCodeProviderService.GetContent(username, project, path);
                });
        }

    }
}
