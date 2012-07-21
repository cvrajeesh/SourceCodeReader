using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using SignalR;
using SourceCodeReader.Web.Hubs;
using SignalR.Hubs;
using SourceCodeReader.Web.Models;
using SourceCodeReader.Web.Infrastructure;

namespace SourceCodeReader.Web.Services
{
    public class DefaultSourceCodeOpeningProgressListener : ClientCallback, ISourceCodeOpeningProgress
    {

        public void OnFindProjectStarted()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Finding the selected project" });
        }

        public void OnProjectFound()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Project found, ready to process" });
        }

        public void OnProjectNotFound()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.NotFound, Message = "Project not found" });
        }

        public void OnProjectDownloadStarted()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Downloading the latest source code" });
        }

        public void OnProjectDownloadCompleted()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Source code downloaded" });
        }

        public void OnProjectDownloadFailed()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.Error, Message = "Downloading source code failed" });
        }

        public void OnProjectLoaded()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.Completed, Message = "Opening project" });
        }


        public void OnProjectLoadingError()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.Error, Message = "An error has occured while loading the project." });
        }


        public void OnProjectPreparing()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Message = "Loading the project" });
        }


        public void OnBuildingWorkspace()
        {
            this.Caller.projectStatus(new ProjectClientStatus { Status = ProjectStatus.InProgress, Message = "Loding the workspace" });
        }
    }
}