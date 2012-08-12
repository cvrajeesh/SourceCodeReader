using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace SourceCodeReader.Web.Infrastructure
{
    public class ApplicationConfigurationProvider : IApplicationConfigurationProvider
    {
        public string ApplicationRoot
        {
            get
            {
                return HostingEnvironment.ApplicationPhysicalPath;
            }
        }

        public string ApplicationDataRoot
        {
            get
            {
                return Path.Combine(this.ApplicationRoot, "App_Data");               
            }
        }

        public string SourceCodeIndexPath
        {
            get
            {
                return Path.Combine(this.ApplicationDataRoot, "SourceCodeIndex");
            }
        }

        public string ProjectsRoot
        {
            get
            {
                return ApplicationDataRoot;
            }
        }

        public string GetProjectSourceCodePath(string username, string project)
        {
            return Path.Combine(ProjectsRoot, string.Format("{0}-{1}", username, project));
        }

        public string GetProjectPackagePath(string username, string project)
        {
            return Path.Combine(ProjectsRoot, string.Format("{0}_{1}.zip", username, project));
        }
    }
}