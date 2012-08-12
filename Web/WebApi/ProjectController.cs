using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SourceCodeReader.Web.Models;
using System.Threading.Tasks;
using SourceCodeReader.Web.Services;
using SourceCodeReader.Web.Infrastructure;

namespace SourceCodeReader.Web.WebApi
{
    public class ProjectController : ApiController
    {
        private ISourceCodeProviderService sourceCodeProviderService;
        private ISourceCodeOpeningProgress openingProgressListener;

        public ProjectController(ISourceCodeProviderService sourceCodeProviderService, ISourceCodeOpeningProgress openingProgressListener)
        {
            this.sourceCodeProviderService = sourceCodeProviderService;
            this.openingProgressListener = openingProgressListener;
        }

        [WebApiOutputCache(120, 60)]
        public Task<ProjectItem> Get(string username, string project, string path)
        {
            return Task.Factory.StartNew<ProjectItem>(() =>
                {
                    if (this.openingProgressListener is IClientCallback)
                    {
                        ((IClientCallback)this.openingProgressListener).ProjectConnectionId = ControllerContext.Request.Cookie("ProjectConnectionId");
                    }

                    return this.sourceCodeProviderService.GetContent(username, project, path, this.openingProgressListener);
                });
        }

    }
}
