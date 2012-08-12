using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR;

namespace SourceCodeReader.Web.Hubs
{
    public class ProjectConnectionIdGenerator : IConnectionIdGenerator
    {
        public string GenerateConnectionId(IRequest request)
        {
            var connectionCookie = request.Cookies["ProjectConnectionId"];
            if (connectionCookie != null)
            {
                return connectionCookie.Value;
            }

            return Guid.NewGuid().ToString();
        }
    }
}