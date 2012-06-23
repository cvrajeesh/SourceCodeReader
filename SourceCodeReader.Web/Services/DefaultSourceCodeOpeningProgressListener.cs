using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using SignalR;
using SourceCodeReader.Web.Hubs;
using SignalR.Hubs;
using SourceCodeReader.Web.Models;

namespace SourceCodeReader.Web.Services
{
    public class DefaultSourceCodeOpeningProgressListener : ISourceCodeOpeningProgress
    {
        private IHubContext context;

        public DefaultSourceCodeOpeningProgressListener()
        {
            this.context = GlobalHost.ConnectionManager.GetHubContext<ProjectHub>();            
        }

        public void OnFindProjectStarted()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Finding the selected project" });
        }

        public void OnProjectFound()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Project found, ready to process" });
        }

        public void OnProjectNotFound()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.NotFound, Message = "Project not found" });
        }

        public void OnProjectDownloadStarted()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Downloading the latest source code" });
        }

        public void OnProjectDownloadCompleted()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Source code downloaded" });
        }

        public void OnProjectDownloadFailed()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.Error, Message = "Downloading source code failed" });
        }

        public void OnProjectLoaded()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.Completed, Message = "Opening project" });
        }


        public void OnProjectLoadingError()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Status = ProjectStatus.Error, Message = "Ab error has occured while loading the project." });
        }


        public void OnProjectPreparing()
        {
            this.context.Clients.projectStatus(new ProjectClientStatus { Message = "Loading the project" });
        }
    }
}