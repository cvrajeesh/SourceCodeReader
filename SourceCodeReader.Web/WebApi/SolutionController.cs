using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SourceCodeReader.Web.Services;
using SourceCodeReader.Web.Models;
using System.Threading.Tasks;
using SourceCodeReader.Web.LanguageServices;
using SourceCodeReader.Web.Infrastructure;

namespace SourceCodeReader.Web.WebApi
{
    public class SolutionController : ApiController
    {
        private IEditorService editorService;
        private IFindReferenceProgress findProgressListener;

        public SolutionController(IEditorService editorService, IFindReferenceProgress findProgressListener)
        {
            this.editorService = editorService;
            this.findProgressListener = findProgressListener;
        }

        [HttpPost]
        public Task<FindReferenceResult> GoToDefinition(FindReferenceParameter findReferenceParameter)
        {
            return Task.Factory.StartNew<FindReferenceResult>(() =>
            {
                if (this.findProgressListener is IClientCallback)
                {
                    ((IClientCallback)this.findProgressListener).ProjectConnectionId = ControllerContext.Request.Cookie("ProjectConnectionId");
                }

                return this.editorService.GoToDefinition(findReferenceParameter, findProgressListener);
            });
        }

        [HttpPost]
        public Task<List<FindReferenceResult>> FindReferences(FindReferenceParameter findReferenceParameter)
        {
            return Task.Factory.StartNew<List<FindReferenceResult>>(() =>
                {
                    if (this.findProgressListener is IClientCallback)
                    {
                        ((IClientCallback)this.findProgressListener).ProjectConnectionId = ControllerContext.Request.Cookie("ProjectConnectionId");
                    }

                    return this.editorService.FindRefernces(findReferenceParameter, findProgressListener);
                });
        }
    }
}
