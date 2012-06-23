using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceCodeReader.Web.Services.GitHub;
using Ninject.Activation;
using SourceCodeReader.Web.Services;
using System.Net.Http;
using System.Web.Routing;
using System.Web.Http;

namespace SourceCodeReader.Web.Infrastructure
{
    public class SourceCodeProviderServiceNinjectProvider : Provider<GitHubSourceCodeProviderService>
    {
        protected override GitHubSourceCodeProviderService CreateInstance(IContext context)
        {
            
            var httpRequestMessage = context.Kernel.GetService(typeof(HttpRequestMessage)) as HttpRequestMessage;


            return new GitHubSourceCodeProviderService(
                context.Kernel.GetService(typeof(ISourceCodeOpeningProgress)) as ISourceCodeOpeningProgress,
                httpRequestMessage.GetRouteData().Values["username"].ToString());

        }
    }
}