using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.Services
{
    public interface IProjectDiscoveryService
    {
        Project FindProject(string username, string projectName);
    }
}