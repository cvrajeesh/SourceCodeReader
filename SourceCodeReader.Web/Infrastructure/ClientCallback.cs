using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR;
using SignalR.Hubs;
using SourceCodeReader.Web.Hubs;

namespace SourceCodeReader.Web.Infrastructure
{
    public abstract class ClientCallback : IClientCallback
    {
        private IHubContext context;

        public string ProjectConnectionId { get; set; }

        public dynamic Caller
        {
            get
            {
                if (this.context == null)
                {
                    this.context = GlobalHost.ConnectionManager.GetHubContext<ProjectHub>();
                }

                if (!string.IsNullOrEmpty(ProjectConnectionId))
                {
                    return this.context.Clients[ProjectConnectionId];
                }

                return this.context.Clients;
            }
        }
    }
}