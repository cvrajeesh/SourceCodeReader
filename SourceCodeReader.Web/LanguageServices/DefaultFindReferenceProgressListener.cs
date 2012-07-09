using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using SignalR;
using SourceCodeReader.Web.Hubs;
using SourceCodeReader.Web.Infrastructure;

namespace SourceCodeReader.Web.LanguageServices
{
    public class DefaultFindReferenceProgressListener : ClientCallback, IFindReferenceProgress
    {

        public void OnFindReferenceStarted()
        {
            this.Caller.findReferenceStatus("Searching initiated.");
        }

        public void OnFindReferenceInProgress()
        {
            this.Caller.findReferenceStatus("Searching in progress...");
        }

        public void OnFindReferenceCompleted(int searchResultCount)
        {
            this.Caller.findReferenceStatus(string.Format("Searching completed with {0} results", searchResultCount));
        }
    }
}