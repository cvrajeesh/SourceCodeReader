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

namespace SourceCodeReader.Web.WebApi
{
    public class SolutionController : ApiController
    {
        private IEditorService editorService;

        public SolutionController(IEditorService editorService)
        {
            this.editorService = editorService;
        }


        [HttpPost]
        public Task<List<FindReferenceResult>> FindReferences(FindReferenceParameter findReferenceParameter)
        {
            return Task.Factory.StartNew<List<FindReferenceResult>>(() =>
                {
                    return this.editorService.FindRefernces(findReferenceParameter);
                });
        }
    }
}
