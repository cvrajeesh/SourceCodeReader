using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceCodeReader.Web.Infrastructure
{
    public interface IClientCallback
    {
        string ProjectConnectionId { get; set; }
    }
}
