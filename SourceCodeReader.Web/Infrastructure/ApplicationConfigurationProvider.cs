using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;

namespace SourceCodeReader.Web.Infrastructure
{
    public class ApplicationConfigurationProvider : IApplicationConfigurationProvider
    {
        public string AWSAccessID 
        { 
            get
            {
                return ConfigurationManager.AppSettings["AWSAccessID"];
            }
        }

        public string AWSSecretAccessKey
        {
            get
            {
                return ConfigurationManager.AppSettings["AWSSecretAccessKey"];
            }
        }

        public string AWSBlogBucket
        {
            get
            {
                return ConfigurationManager.AppSettings["AWSBlogBucket"];
            }
        }

        public string ApplicationRoot
        {
            get
            {
                return System.IO.Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
            }
        }

        public string ProjectsRoot
        {
            get
            {
                return Path.Combine(ApplicationRoot, "Projects");
            }
        }

        public string GetProjectPath(string username, string project)
        {
            return Path.Combine(ProjectsRoot, username, project);
        }

        public string GetProjectSourceCodePath(string username, string project)
        {
            return Path.Combine(GetProjectPath(username, project), "Source");
        }

        public string GetProjectPackagePath(string username, string project)
        {
            return Path.Combine(ProjectsRoot, string.Format("{0}_{1}.zip", username, project));
        }
    }
}