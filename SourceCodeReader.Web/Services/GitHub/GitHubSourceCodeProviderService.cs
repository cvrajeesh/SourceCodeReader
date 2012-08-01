using System;
using System.IO;
using System.Net;
using Ionic.Zip;
using SourceCodeReader.Web.Infrastructure;
using SourceCodeReader.Web.LanguageServices;
using SourceCodeReader.Web.Models;
using Ninject.Extensions.Logging;
using System.Collections.Concurrent;

namespace SourceCodeReader.Web.Services.GitHub
{
    public class GitHubSourceCodeProviderService : ISourceCodeProviderService
    {
        private IProjectDiscoveryService projectDiscoveryService;
        private IApplicationConfigurationProvider applicationConfigurationProvider;
        private IEditorService editorService;
        private ISourceCodeIndexingService sourceCodeIndexingService;
        private ILogger logger;
        private static readonly ConcurrentDictionary<string, object> ProjectDownloadLock = new ConcurrentDictionary<string, object>();

        public GitHubSourceCodeProviderService(
            IProjectDiscoveryService projectDiscoveryService,
            IApplicationConfigurationProvider applicationConfigurationProvider,
            IEditorService editorService,
            ISourceCodeIndexingService sourceCodeIndexingService,
            ILogger logger)
        {
            this.projectDiscoveryService = projectDiscoveryService;
            this.applicationConfigurationProvider = applicationConfigurationProvider;
            this.editorService = editorService;
            this.sourceCodeIndexingService = sourceCodeIndexingService;
            this.logger = logger;
        }
        

        public ProjectItem GetContent(string username, string project, string path, ISourceCodeOpeningProgress openingProgressListener)
        {
            var projectSourceCodePath = this.applicationConfigurationProvider.GetProjectSourceCodePath(username, project);
            try
            {
                if (!Directory.Exists(projectSourceCodePath))
                {
                    string projectIdentifier = string.Format("{0}_{1}", username, project);
                    object __projectSpecificLock = ProjectDownloadLock.GetOrAdd(projectIdentifier, new object());
                    lock (__projectSpecificLock)
                    {
                        if (!Directory.Exists(projectSourceCodePath))
                        {
                            string packagePath = this.applicationConfigurationProvider.GetProjectPackagePath(username, project);
                            if (!File.Exists(packagePath))
                            {
                                openingProgressListener.OnFindProjectStarted();
                                var projectSelected = this.projectDiscoveryService.FindProject(username, project);
                                if (projectSelected == null)
                                {
                                    openingProgressListener.OnProjectNotFound();
                                    return null;
                                }
                                openingProgressListener.OnProjectFound();
                                
                                openingProgressListener.OnProjectDownloadStarted();
                                var downloadStatus = this.DownloadZipBall(packagePath, projectSelected.DownloadPackageUrl);
                                if (!downloadStatus)
                                {
                                    openingProgressListener.OnProjectDownloadFailed();
                                    return null;
                                }

                                openingProgressListener.OnProjectDownloadCompleted();
                            }

                            openingProgressListener.OnProjectPreparing();
                            this.ExtractZipBall(packagePath, projectSourceCodePath);

                            this.editorService.RewriteExternalDependencies(username, project);
                        }
                    }

                    // Try to remove the created lock object
                    ProjectDownloadLock.TryRemove(projectIdentifier, out __projectSpecificLock);
                }

                openingProgressListener.OnBuildingWorkspace();                
                this.sourceCodeIndexingService.IndexProject(username, project, projectSourceCodePath);
                openingProgressListener.OnProjectLoaded();

                var projectItem = GetContentFromPath(username, project, projectSourceCodePath, path);
                projectItem.DownloadedDate = Directory.GetCreationTimeUtc(projectSourceCodePath).ToString("dd MMM yyyy hh:mm:ss UTC");
                return projectItem;
            }
            catch (Exception ex)
            {
                openingProgressListener.OnProjectLoadingError();
                this.logger.Error(ex, "An error has occured while getting the content for path {0} from project {1}/{2}", path, username, project);
                return null;
            }
        }

        private ProjectItem GetContentFromPath(string username, string project, string projectSourceCodePath, string path)
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
                        fileItem.Content = this.editorService.BuildNavigatableSourceCodeFromFile(username, project, path);
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