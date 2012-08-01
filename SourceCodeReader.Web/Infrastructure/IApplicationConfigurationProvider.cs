using System;
namespace SourceCodeReader.Web.Infrastructure
{
    public interface IApplicationConfigurationProvider
    {
        string ApplicationRoot { get; }

        string ApplicationDataRoot { get; }

        //string GetProjectPath(string username, string project);

        string GetProjectPackagePath(string username, string project);

        string GetProjectSourceCodePath(string username, string project);

        string ProjectsRoot { get; }

        string SourceCodeIndexPath { get; }       
    }
}
