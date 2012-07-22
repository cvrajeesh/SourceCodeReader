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
                return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data");               
            }
        }

        public string SourceCodeIndexPath
        {
            get
            {
                return Path.Combine(this.ApplicationRoot, "SourceCodeIndex");
            }
        }

        public string ProjectsRoot
        {
            get
            {
                return ApplicationRoot;
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