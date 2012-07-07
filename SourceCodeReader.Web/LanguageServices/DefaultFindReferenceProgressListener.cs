using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using SignalR;
using SourceCodeReader.Web.Hubs;

namespace SourceCodeReader.Web.LanguageServices
{
    public class DefaultFindReferenceProgressListener : IFindReferenceProgress
    {
        private IHubContext context;

        public DefaultFindReferenceProgressListener()
        {
            this.context = GlobalHost.ConnectionManager.GetHubContext<ProjectHub>();   
        }

        public void OnFindReferenceStarted()
        {
            this.context.Clients.findReferenceStatus("Searching initiated.");
        }

        public void OnFindReferenceInProgress()
        {
            this.context.Clients.findReferenceStatus("Searching in progress...");
        }

        public void OnFindReferenceCompleted(int searchResultCount)
        {
            this.context.Clients.findReferenceStatus(string.Format("Searching completed with {0} results",searchResultCount));
        }
    }
}