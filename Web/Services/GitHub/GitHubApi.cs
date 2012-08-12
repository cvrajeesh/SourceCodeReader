using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestSharp;

namespace  SourceCodeReader.Web.Services.GitHub
{
    public class GitHubApi
    {
        private const string baseUrl = @"https://api.github.com";
        private string username;
        private string repository;

        public GitHubApi(string username, string repository)
        {
            this.username = username;
            this.repository = repository;
        }

        public T Execute<T>(RestRequest request) where T:new()
        {
            var client = new RestClient(baseUrl);
            request.AddUrlSegment("username", username);
            request.AddUrlSegment("repository", repository);
            var response = client.Execute<T>(request);
            return response.Data;
        }
    }
}