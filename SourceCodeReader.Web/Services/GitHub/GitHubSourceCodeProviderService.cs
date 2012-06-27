using System;
using System.IO;
using System.Net;
using Ionic.Zip;
using SourceCodeReader.Web.Infrastructure;
using SourceCodeReader.Web.LanguageServices;
using SourceCodeReader.Web.Models;
using Ninject.Extensions.Logging;

namespace SourceCodeReader.Web.Services.GitHub
{
    public class GitHubSourceCodeProviderService : ISourceCodeProviderService
    {
        private ISourceCodeOpeningProgress openingProgress;
        private IProjectDiscoveryService projectDiscoveryService;
        private IApplicationConfigurationProvider applicationConfigurationProvider;
        private IEditorService editorService;
        private ILogger logger;

        // TODO: Feels like too much dependencies, think about refactoring if needed
        public GitHubSourceCodeProviderService(
            ISourceCodeOpeningProgress openingProgress, 
            IProjectDiscoveryService projectDiscoveryService,
            IApplicationConfigurationProvider applicationConfigurationProvider,
            IEditorService editorService,
            ILogger logger)
        {
            this.openingProgress = openingProgress;
            this.projectDiscoveryService = projectDiscoveryService;
            this.applicationConfigurationProvider = applicationConfigurationProvider;
            this.editorService = editorService;
            this.logger = logger;
        }

        public ProjectItem GetContent(string username, string project, string path)
        {
            var projectSourceCodePath = this.applicationConfigurationProvider.GetProjectSourceCodePath(username, project);
            try
            {
                if (!Directory.Exists(projectSourceCodePath))
                {
                    string packagePath = this.applicationConfigurationProvider.GetProjectPackagePath(username, project);
                    if (!File.Exists(packagePath))
                    {
                        this.openingProgress.OnFindProjectStarted();
                        var projectSelected = this.projectDiscoveryService.FindProject(username, project);
                        if (projectSelected == null)
                        {
                            this.openingProgress.OnProjectNotFound();
                            return null;
                        }
                        this.openingProgress.OnProjectFound();
                        this.applicationConfigurationProvider.ProjectsRoot.EnsureDirectoryExists();
                        this.openingProgress.OnProjectDownloadStarted();
                        var downloadStatus = this.DownloadZipBall(packagePath, projectSelected.DownloadPackageUrl);
                        if (!downloadStatus)
                        {
                            this.openingProgress.OnProjectDownloadFailed();
                            return null;
                        }

                        this.openingProgress.OnProjectDownloadCompleted();
                        this.openingProgress.OnProjectPreparing();
                        this.ExtractZipBall(packagePath, projectSourceCodePath);
                        this.openingProgress.OnProjectLoaded();
                    }

                }

                return GetContentFromPath(projectSourceCodePath, path);
            }
            catch (Exception ex)
            {
                this.openingProgress.OnProjectLoadingError();
                this.logger.Error(ex, "An error has occured while getting the content for path {0} from project {1}/{2}", path, username, project);
                return null;
            }
        }

        private ProjectItem GetContentFromPath(string projectSourceCodePath, string path)
        {
            var repositoryDirectory = new DirectoryInfo(projectSourceCodePath);
            DirectoryInfo applicationRoot = repositoryDirectory.GetDirectories()[0];
            string currentDirectoryName = "root";

            if (!string.IsNullOrEmpty(path))
            {
                // Remove the trailing slash to support *.cs files
                if (path.EndsWith("/"))
                {
                    path = path.Substring(0, path.Length - 1);
                }

                if (Path.IsPathRooted(path))
                {
                    return GetDirectoryContent(string.Empty, applicationRoot, currentDirectoryName);
                }
                else
                {

                    var fullPath = Path.Combine(applicationRoot.FullName, path);
                    if (fullPath.IsFilePath())
                    {
                        // Get the file content
                        var fileItem = new ProjectItem { Type = ProjectItemType.File };
                        fileItem.Path = path;
                        fileItem.Name = Path.GetFileName(fullPath);
                        fileItem.Content = this.editorService.BuildNavigatableSourceCodeFromFile(fullPath);
                        return fileItem;
                    }
                    else
                    {
                        applicationRoot = new DirectoryInfo(fullPath);
                        currentDirectoryName = applicationRoot.Name;
                        return GetDirectoryContent(path, applicationRoot, currentDirectoryName);
                    }
                }
            }
            else
            {
                return GetDirectoryContent(string.Empty, applicationRoot, currentDirectoryName);
            }

        }

        private ProjectItem GetDirectoryContent(string path, DirectoryInfo applicationRoot, string currentDirectoryName)
        {
            var rootDirectory = new ProjectItem();
            rootDirectory.Type = ProjectItemType.Directory;
            rootDirectory.Name = currentDirectoryName;
            rootDirectory.Path = path;
            foreach (var directory in applicationRoot.GetDirectories())
            {
                var repositoryItem = new ProjectItem();
                repositoryItem.Name = directory.Name;
                repositoryItem.Path = Path.Combine(path, directory.Name).Replace(@"\", "/");
                repositoryItem.Type = ProjectItemType.Directory;
                rootDirectory.Items.Add(repositoryItem);
            }

            foreach (var file in applicationRoot.GetFiles())
            {
                var repositoryItem = new ProjectItem();
                repositoryItem.Name = file.Name;
                repositoryItem.Path = Path.Combine(path, file.Name).Replace(@"\", "/"); ;
                repositoryItem.Type = ProjectItemType.File;
                rootDirectory.Items.Add(repositoryItem);
            }

            return rootDirectory;
        }

        private bool DownloadZipBall(string filePath, Uri zipBallUri)
        {
            var downloadRequest = WebRequest.Create(zipBallUri);
            var downloadResponse = downloadRequest.GetResponse();
            if (downloadResponse != null)
            {
                var responseStream = downloadResponse.GetResponseStream();

                // Create the local file
                using (var localStream = File.Create(filePath))
                {
                    // Allocate a 1k buffer
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    // Simple do/while loop to read from stream until
                    // no bytes are returned
                    do
                    {
                        // Read data (up to 1k) from the stream
                        bytesRead = responseStream.Read(buffer, 0, buffer.Length);

                        // Write the data to the local file
                        localStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead > 0);
                }

                return true;
            }

            return false;
        }

        private void ExtractZipBall(string zipFileToExtract, string destinationDirectory)
        {
            //Clean up the existing repository
            if (Directory.Exists(destinationDirectory))
            {
                Directory.Delete(destinationDirectory, true);
            }

            using (var zipFile = ZipFile.Read(zipFileToExtract))
            {
                zipFile.ExtractAll(destinationDirectory, ExtractExistingFileAction.OverwriteSilently);
            }
        }
    
    }
}