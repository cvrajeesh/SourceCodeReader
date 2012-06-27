using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceCodeReader.Web.Models;
using RestSharp;
using Ninject.Extensions.Logging;

namespace SourceCodeReader.Web.Services.GitHub
{
    public class GitHubProjectDiscoveryService : IProjectDiscoveryService
    {
        private ILogger logger;

        public GitHubProjectDiscoveryService(ILogger logger)
        {
            this.logger = logger;
        }

        public Project FindProject(string username, string projectName)
        {
            var gitHubApi = new GitHubApi(username, projectName);
            var request = new RestRequest(@"repos/{username}/{repository}");
            try
            {
                var responseData = gitHubApi.Execute<GitHubRepo>(request);

                string masterBranch = "master";
                if (!string.IsNullOrEmpty(responseData.Master_Branch))
                {
                    masterBranch = responseData.Master_Branch;
                }

                return new Project
                {
                     Name = responseData.Full_Name,
                     DownloadPackageUrl = new Uri(string.Format(@"https://github.com/{0}/{1}/zipball/{2}", username, projectName, masterBranch)),
                     LastModified = DateTime.UtcNow,
                     Description = responseData.Description
                };
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "An error has occured when finding project {0}/{1}", username, projectName);
                return null;
            }
        }
    }
}